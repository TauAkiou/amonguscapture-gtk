using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Gtk;

namespace AmongUsCapture
{
    public partial class MainForm : Window
    {
        public MainForm() : base("MainForm")
        {
            InitializeWindow();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            
            GLib.Idle.Add( delegate {
                label2.Text = e.NewState.ToString();
                return false;
            });
            
        }
    }

}
