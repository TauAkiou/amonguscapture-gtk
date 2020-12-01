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
        
        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            if (e.Action == PlayerAction.Died)
            {
                DeadMessages.Enqueue($"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
            }
            else
            {
                AmongUsCapture.Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki,$"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
            }
            
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }
        
        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Sender}{NormalTextColor.ToTextColor()}: {e.Message}");
            //WriteLineToConsole($"[CHAT] {e.Sender}: {e.Message}");
        }
        
        private void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            _gameStatusCodeEntry.Text = e.LobbyCode;
            GameCodeBox.BeginInvoke(a => a.Text = e.LobbyCode);
        }
        
        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            setCurrentState(e.NewState.ToString());
            while (DeadMessages.Count > 0)
            {
                AmongUsCapture.Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, DeadMessages.Dequeue());
            }
            
            AmongUsCapture.Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime,
                $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }
        
        private async void ReloadOffsetsButton_OnClick(object sender, RoutedEventArgs e)
        {
            GameMemReader.getInstance().offMan.refreshLocal();
            await GameMemReader.getInstance().offMan.RefreshIndex();
            GameMemReader.getInstance().CurrentOffsets = GameMemReader.getInstance().offMan
                .FetchForHash(GameMemReader.getInstance().GameHash);
            if (GameMemReader.getInstance().CurrentOffsets is not null)
            {
                WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"Loaded offsets: {GameMemReader.getInstance().CurrentOffsets.Description}");
            }
            else
            {
                WriteConsoleLineFormatted("GameMemReader", Color.Lime, $"No offsets found for: {Color.Aqua.ToTextColor()}{GameMemReader.getInstance().GameHash.ToString()}.");

            }
        }
        
        public void setCurrentState(string state)
        {
            StatusBox.BeginInvoke(tb => { tb.Text = state; });
        }
        
        public void WriteConsoleLineFormatted(string moduleName, Color moduleColor, string message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            // Use Pango markup, which is slightly different.
            WriteColoredText(
                $"[{moduleColor.ToPangoColor(moduleName)}]: {message}");
        }
        
        private Color PlayerColorToColorOBJ(PlayerColor pColor)
        {
            var OutputCode = Color.White;
            switch (pColor)
            {
                case PlayerColor.Red:
                    OutputCode = Color.Red;
                    break;
                case PlayerColor.Blue:
                    OutputCode = Color.RoyalBlue;
                    break;
                case PlayerColor.Green:
                    OutputCode = Color.Green;
                    break;
                case PlayerColor.Pink:
                    OutputCode = Color.Magenta;
                    break;
                case PlayerColor.Orange:
                    OutputCode = Color.Orange;
                    break;
                case PlayerColor.Yellow:
                    OutputCode = Color.Yellow;
                    break;
                case PlayerColor.Black:
                    OutputCode = Color.Gray;
                    break;
                case PlayerColor.White:
                    OutputCode = Color.White;
                    break;
                case PlayerColor.Purple:
                    OutputCode = Color.MediumPurple;
                    break;
                case PlayerColor.Brown:
                    OutputCode = Color.SaddleBrown;
                    break;
                case PlayerColor.Cyan:
                    OutputCode = Color.Cyan;
                    break;
                case PlayerColor.Lime:
                    OutputCode = Color.Lime;
                    break;
            }

            return OutputCode;
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