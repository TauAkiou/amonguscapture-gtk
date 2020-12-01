using System;
using System.Collections.Generic;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using Gtk;
using GLib;
using Gdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Color = System.Drawing.Color;
using Window = Gtk.Window;

namespace AUCapture_GTK.GTK
{
    public partial class PrimaryWindow : Window
    {
        //public UserDataContext context;
        private bool connected;
        private readonly object locker = new object();
        private Queue<string> DeadMessages = new Queue<string>();
        public static Color NormalTextColor = Color.Black;

        
        public PrimaryWindow() : base("Among Us - GTK")
        {
            window_initialize();
            NormalTextColor = GetRgbColorFromFloat(_consoleTextView.StyleContext.GetColor(Gtk.StateFlags.Normal));
            WriteConsoleLineFormatted("Test", Color.Blue, "test");
        }
        
        private void OnGameOver(object? sender, GameOverEventArgs e)
        {
            WriteConsoleLineFormatted("GameOver", Color.BlueViolet, JsonConvert.SerializeObject(e, Formatting.None, new StringEnumConverter()));
            Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
        }
        
        public void WriteConsoleLineFormatted(string moduleName, Color moduleColor, string message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            // Use Pango markup, which is slightly different.
            WriteColoredText(
                $"[{moduleColor.ToPangoColor(moduleName)}]: {message}");
        }

        public void WriteColoredText(string ColoredText, bool addNewLine = false)
        {
            if (_consoleTextView is not null)
            {
                Idle.Add(delegate
                {
                    var iter = _consoleTextView.Buffer.EndIter;
                    _consoleTextView.Buffer.InsertMarkup(ref iter, addNewLine
                        ? $"{ColoredText}{Environment.NewLine}"
                        : $"{ColoredText}");
                    _consoleTextView.Buffer.PlaceCursor(iter);
                    return false;
                });
            }
            
            
        }
        
        private Color GetRgbColorFromFloat(RGBA gtkcolor)
        {
            // it's quick and sloppy, but these are GUI colors and don't have to be horribly accurate.
            return Color.FromArgb((byte)(gtkcolor.Alpha * 255),
                (byte)(gtkcolor.Red * 255),
                (byte)(gtkcolor.Green * 255),
                (byte)(gtkcolor.Blue * 255));

        }
        
        
    }
}