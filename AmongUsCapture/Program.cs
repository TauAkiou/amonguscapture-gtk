using System;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using AmongUsCapture.ConsoleTypes;
using GLib;
using Microsoft.Win32;
using SharedMemory;
using Gtk;
using Application = Gtk.Application;
using Mutex = System.Threading.Mutex;
using Process = System.Diagnostics.Process;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

namespace AmongUsCapture
{
    internal static class Program
    {
        public static UserForm window;
        private static Mutex mutex = null;
        private static ClientSocket socket;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Settings.PersistentSettings.debugConsole)
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs

            if (!Directory.Exists(Settings.StorageLocation))
            {
                // Create Settings directory if it doesn't exist, as we need to stick our pidfile there.
                Directory.CreateDirectory(Settings.StorageLocation);
            }
            
            URIStartResult uriRes = URIStartResult.CLOSE;
            uriRes = IPCadapter.getInstance().HandleURIStart(args);
                switch (uriRes)
                {
                    case URIStartResult.CLOSE:
                        Environment.Exit(0);
                        break;
                    case URIStartResult.PARSE:
                        Console.WriteLine($"Starting with args : {args[0]}");
                        break;
                    case URIStartResult.CONTINUE:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                socket = new ClientSocket();
            
            //Create the Form Console interface. 
            var thread = new Thread(OpenGUI);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            while (Settings.conInterface is null) Thread.Sleep(250);
            Task.Factory.StartNew(() => socket.Init())
                .Wait(); // run socket in background. Important to wait for init to have actually finished before continuing
            Task.Factory.StartNew(() => IPCadapter.getInstance().RegisterMinion()).Wait();

            // Add a GLib Idle handler to fix the issue here. 
            Idle.Add(delegate
            {
                Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
                if (uriRes == URIStartResult.PARSE)
                    IPCadapter.getInstance().SendToken(args[0]);
                return false;
            });

            thread.Join();
        }


        private static void OpenGUI()
        {
            var appstate = new Application("org.AmongUsCapture.AmongUsCaptureUtil", GLib.ApplicationFlags.None);
            appstate.Register(GLib.Cancellable.Current);
            Application.Init();
            window = new UserForm(socket);
            appstate.AddWindow(window);
            Settings.form = window;
            Settings.conInterface = new FormConsole(window);
            
            window.DeleteEvent += (object o, DeleteEventArgs e) =>
            {
                // Make sure that the IPC adapter has a chance to clean up after itself.
                IPCadapter.getInstance().Cancel().Wait();
                Application.Quit();
            };

            window.ShowAll();

            Application.Run();
            IPCadapter.getInstance().Cancel().Wait();
            
            Environment.Exit(0);
        }

        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
        

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
        
    }
}