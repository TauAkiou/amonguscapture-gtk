using System;
using System.Collections.Generic;
using Gtk;
using GLib;
using Gdk;
using Color = System.Drawing.Color;
using Window = Gtk.Window;

namespace AUCapture_GTK.GTK
{
    public partial class PrimaryWindow : Window
    {
        public PrimaryWindow() : base("Among Us - GTK")
        {
            window_initialize();
        }
    }
}