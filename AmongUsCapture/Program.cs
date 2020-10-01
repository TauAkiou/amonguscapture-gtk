using AmongUsCapture.ConsoleTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if(doConsole)
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }
            /* This is winforms stuff and doesn't apply to GTK.
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            */
            
            
            ClientSocket socket = new ClientSocket();

            string hostPath = "host.txt";
          
            //TODO make proper properties file
            string host = File.Exists(hostPath) ? File.ReadAllText(hostPath) : "http://localhost:8123";
            var windowbuilder = new Builder();
            var form = new UserForm(windowbuilder, socket);
            conInterface = new FormConsole(form); //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Connect(host)); //synchronously force the socket to connect
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
            //(new DebugConsole(debugGui)).Run();
            
            appstate.AddWindow(form);
            form.DeleteEvent += (object o, DeleteEventArgs e) =>
            {
                Application.Quit();
            };

            form.ShowAll();
            Application.Run();
            var form = new UserForm(socket);
            Settings.conInterface = new FormConsole(form); //Create the Form Console interface. 
            Task.Factory.StartNew(() => socket.Connect(Settings.PersistentSettings.host)); //synchronously force the socket to connect
            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background

            //AllocConsole();
            Application.Run(form);
            
            //test
        }

        
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        

    }
}
