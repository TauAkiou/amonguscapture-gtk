using System;
using System.Runtime.InteropServices;
using AUCapture_GTK.GTK;
using GLib;
using Gtk;
using Application = Gtk.Application;
using Settings = AmongUsCapture.Settings;

namespace AUCapture_GTK
{
    internal static class Program
    {
        public static PrimaryWindow pwindow;
        
        [STAThread]
        static void Main(string[] args)
        {
            // This is necessary for Windows. Since GTK still does have Windows compatibility,
            // we will leave this line in for support.
            if (Settings.PersistentSettings.debugConsole && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AllocConsole();
            }
            
            var gtkappstate = new Application("org.AmongUsCapture.Gtk", ApplicationFlags.None);
            gtkappstate.Register(Cancellable.Current);
            Application.Init();
            Gtk.Settings.Default.SetLongProperty ("gtk-button-images", 1, "");
            pwindow = new PrimaryWindow();
            gtkappstate.AddWindow(pwindow);

            pwindow.DeleteEvent += (object o, DeleteEventArgs e) =>
            {
                Application.Quit();
            };

            pwindow.ShowAll();
            
            Application.Run();

            Environment.Exit(0);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
    }
}