using AmongUsCapture.ConsoleTypes;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace AmongUsCapture
{
    static class Program
    {
        private static bool debugGui = true;
        public static ConsoleInterface conInterface = null;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if(!debugGui)
            {
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var sock = new ClientSocket();

            var form = new UserForm(sock);
            conInterface = new FormConsole(form); //Create the Form Console interface. 

            Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background; Needs to happen after conInterface is set

            //(new DebugConsole(debugGui)).Run();
            
            Application.Run(form);
            
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

    }
}
