using AmongUsCapture.ConsoleTypes;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Gtk;

namespace AmongUsCapture
{
    static class Program
    {
        private static bool doConsole = false;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var appstate = new Application("org.AmongUsCapture.AmongUsCaptureUtil", GLib.ApplicationFlags.None);
            appstate.Register(GLib.Cancellable.Current);
            Application.Init();
            
            // This particular line is a Win32 system call and should only ever be run there
            if(doConsole && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }

            ClientSocket socket = new ClientSocket();
            
            var windowbuilder = new Builder();
            var form = new UserForm(windowbuilder, socket);
            Settings.conInterface = new FormConsole(form); //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Connect(Settings.PersistentSettings.host)); //synchronously force the socket to connect
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            //(new DebugConsole(debugGui)).Run();
            
            appstate.AddWindow(form);
            form.DeleteEvent += (object o, DeleteEventArgs e) =>
            {
                Application.Quit();
            };

            form.ShowAll();
            Application.Run();
            //test
        }

        
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
