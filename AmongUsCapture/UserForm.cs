using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Gdk;
using GLib;
using Gtk;
using Color = System.Drawing.Color;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;
using Window = Gtk.Window;
using TextColorLibrary;


namespace AmongUsCapture
{
    public partial class UserForm : Window
    {
        private ClientSocket clientSocket;
        private static Atom _atom = Atom.Intern("CLIPBOARD", false);
        private Clipboard _clipboard = Clipboard.Get(_atom);

        public UserForm(Builder builder, ClientSocket sock) : base("Among Us Capture - GTK")
        public static Color NormalTextColor = Color.Black;
        private Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }
        public UserForm(ClientSocket sock)
        {
            //builder.Autoconnect(this);
            SetIconFromFile("icon.ico");
            clientSocket = sock;
            InitializeWindow();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
            
        }

        private void consoleTextView_OnRightClick(object o, ButtonPressEventArgs e)
        {
            if (e.Event.Button == 3)
            {
                Menu menu = new Menu();
                MenuItem menu_item = new MenuItem("Autoscroll");
                menu_item.Add(_autoScrollMenuItem);
                menu.ShowAll();
                menu.PopupAtWidget(menu, Gravity.South, Gravity.East, null);
            }
            NormalTextColor = DarkTheme() ? Color.White : Color.Black;
        }
        
        private void OnJoinedLobby(object sender, LobbyEventArgs e)
        {
            Idle.Add(delegate
            {
                _gameCodeEntryField.Text = e.LobbyCode;
                return false;
            });

        }
        
        private void OnLoad(object sender, EventArgs e)
        {
            TestFillConsole(25);
        }

        private string getRainbowText(string nonRainbow)
        {
            string OutputString = "";
            for (int i = 0; i < nonRainbow.Length; i++)
            {
                OutputString += Rainbow((float)i / nonRainbow.Length).ToTextColor() + nonRainbow[i];
            }
            return OutputString;
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki, $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Sender}{NormalTextColor.ToTextColor()}: {e.Message}");
            //WriteLineToConsole($"[CHAT] {e.Sender}: {e.Message}");
        }
        
        /*
         
         /* GTK uses its own theming, so if you have a dark theme it will be used
            automagically. */
         
        /*
        private bool DarkTheme()
        {
            bool is_dark_mode = false;
            try
            {
                var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
                if (v != null && v.ToString() == "0")
                    is_dark_mode = true;
            }
            catch { }
            return is_dark_mode;
        }

        private void EnableDarkTheme()
        {
            var BluePurpleAccent = Color.FromArgb(114, 137, 218);
            var White = Color.White;
            var AlmostWhite = Color.FromArgb(153, 170, 181);
            var LighterGrey = Color.FromArgb(44, 47, 51);
            var DarkGrey = Color.FromArgb(35, 39, 42);

            ConsoleTextBox.BackColor = LighterGrey;
            ConsoleTextBox.ForeColor = White;

            ConsoleGroupBox.BackColor = DarkGrey;
            ConsoleGroupBox.ForeColor = White;

            UserSettings.BackColor = DarkGrey;
            UserSettings.ForeColor = White;

            CurrentStateGroupBox.BackColor = LighterGrey;
            CurrentStateGroupBox.ForeColor = White;

            ConnectCodeGB.BackColor = LighterGrey;
            ConnectCodeGB.ForeColor = White;

            ConnectCodeBox.BackColor = DarkGrey;
            ConnectCodeBox.ForeColor = White;

            SubmitButton.BackColor = BluePurpleAccent;
            SubmitButton.ForeColor = White;

            GameCodeBox.BackColor = DarkGrey;
            GameCodeBox.ForeColor = White;

            GameCodeGB.BackColor = LighterGrey;
            GameCodeGB.ForeColor = White;

            GameCodeCopyButton.BackColor = BluePurpleAccent;
            GameCodeCopyButton.ForeColor = White;

            BackColor = DarkGrey;
            ForeColor = White;
        }
        */

        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {

            Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, $"{PlayerColorToColorOBJ(e.Color).ToTextColor()}{e.Name}{NormalTextColor.ToTextColor()}: {e.Action}");
            this.ShowAll();
            
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            Idle.Add(delegate
            {
                _currentStateLabel.Text = e.NewState.ToString();
                return false;
            });

            Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"State changed to {Color.Cyan.ToTextColor()}{e.NewState}");
            this.ShowAll();
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }

        private void _connectCodeSubmitButton_Click(object sender, EventArgs e)
        {
            if (_connectCodeEntryField.TextLength == 6)
            {
                clientSocket.SendConnectCode(_connectCodeEntryField.Text);
                //ConnectCodeBox.Enabled = false;
                //SubmitButton.Enabled = false;
                this.ShowAll();
            }
        }

        private void ConsoleTextBox_TextChanged(object sender, EventArgs e)
        { /*
            if (AutoScrollMenuItem.Checked)
            {
                ConsoleTextBox.SelectionStart = ConsoleTextBox.Text.Length;
                ConsoleTextBox.ScrollToCaret();
            }
            */
        }

        private void TestFillConsole(int entries) //Helper test method to see if filling console works.
        {

            //for (int i = 0; i < entries; i++)
            //{
            //    this.WriteConsoleLineFormatted("Rainbow", Rainbow((float)i / entries), getRainbowText("Wow! " + Rainbow((float)i / entries).ToString()));
            //};
            //this.WriteColoredText(getRainbowText("This is a Pre-Release from Carbon's branch."));

        }

        public void WriteConsoleLineFormatted(String moduleName, Color moduleColor, String message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}

           this.WriteColoredText($"[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
            this.ShowAll();
 
        }

        public void WriteColoredText(String ColoredText)
        {
            foreach (var part in TextColor.toParts(ColoredText))
            {
                this.AppendColoredTextToConsole(part.text, part.textColor, false);
            }
            this.AppendColoredTextToConsole("", Color.White, true);
        }

        public void AppendColoredTextToConsole(String line, Color color, bool addNewLine = false)
        {
            if (!(_consoleTextView is null))
            {
                Idle.Add(delegate
                {
                    var iter = _consoleTextView.Buffer.EndIter;
                    _consoleTextView.Buffer.Insert(ref iter, addNewLine
                        ? $"{line}{Environment.NewLine}"
                        : line);
                    _consoleTextView.Buffer.PlaceCursor(iter);
                return false;
                });
                this.ShowAll();
            }
        }

        public void WriteLineToConsole(String line)
        {
            if (!(_consoleTextView is null))
            {
                Idle.Add(delegate
                {
                    var iter = _consoleTextView.Buffer.EndIter;
                    _consoleTextView.Buffer.Insert(ref iter,line + "\n");
                    _consoleTextView.Buffer.PlaceCursor(iter);
                    return false;
                });
                this.ShowAll();
            }
        }

        private Color PlayerColorToColorOBJ(PlayerColor pColor)
        {
            Color OutputCode = Color.White;
            switch (pColor)
            {
                case PlayerColor.Red: OutputCode = Color.Red; break;
                case PlayerColor.Blue: OutputCode = Color.RoyalBlue; break;
                case PlayerColor.Green: OutputCode = Color.Green; break;
                case PlayerColor.Pink: OutputCode = Color.Magenta; break;
                case PlayerColor.Orange: OutputCode = Color.Orange; break;
                case PlayerColor.Yellow: OutputCode = Color.Yellow; break;
                case PlayerColor.Black: OutputCode = Color.Gray; break;
                case PlayerColor.White: OutputCode = Color.White; break;
                case PlayerColor.Purple: OutputCode = Color.MediumPurple; break;
                case PlayerColor.Brown: OutputCode = Color.SaddleBrown; break;
                case PlayerColor.Cyan: OutputCode = Color.Cyan; break;
                case PlayerColor.Lime: OutputCode = Color.Lime; break;
            }
            return OutputCode;
        }
        private string PlayerColorToColorCode(PlayerColor pColor)
        {
            //Red = 0,
            //Blue = 1,
            //Green = 2,
            //Pink = 3,
            //Orange = 4,
            //Yellow = 5,
            //Black = 6,
            //White = 7,
            //Purple = 8,
            //Brown = 9,
            //Cyan = 10,
            //Lime = 11
            string OutputCode = "";
            switch (pColor)
            {
                case PlayerColor.Red: OutputCode = "§c"; break;
                case PlayerColor.Blue: OutputCode = "§1"; break;
                case PlayerColor.Green: OutputCode = "§2"; break;
                case PlayerColor.Pink: OutputCode = "§d"; break;
                case PlayerColor.Orange: OutputCode = "§o"; break;
                case PlayerColor.Yellow: OutputCode = "§e"; break;
                case PlayerColor.Black: OutputCode = "§0"; break;
                case PlayerColor.White: OutputCode = "§f"; break;
                case PlayerColor.Purple: OutputCode = "§5"; break;
                case PlayerColor.Brown: OutputCode = "§n"; break;
                case PlayerColor.Cyan: OutputCode = "§b"; break;
                case PlayerColor.Lime: OutputCode = "§a"; break;
            }
            return OutputCode;
        }

        public void WriteLineFormatted(string str, bool acceptnewlines = true)
        {
            if (!(_consoleTextView is null))
            {
                Idle.Add(delegate
                {
                    if (!String.IsNullOrEmpty(str))
                    {
                        if (!acceptnewlines)
                        {
                            str = str.Replace('\n', ' ');
                        }
                        string[] parts = str.Split(new char[] { '§' });
                        if (parts[0].Length > 0)
                        {
                            AppendColoredTextToConsole(parts[0], Color.White, false);
                        }
                        for (int i = 1; i < parts.Length; i++)
                        {
                            Color charColor = Color.White;
                            if (parts[i].Length > 0)
                            {
                                switch (parts[i][0])
                                {
                                    case '0': charColor = Color.Gray; break; //Should be Black but Black is non-readable on a black background
                                    case '1': charColor = Color.RoyalBlue; break;
                                    case '2': charColor = Color.Green; break;
                                    case '3': charColor = Color.DarkCyan; break;
                                    case '4': charColor = Color.DarkRed; break;
                                    case '5': charColor = Color.MediumPurple; break;
                                    case '6': charColor = Color.DarkKhaki; break;
                                    case '7': charColor = Color.Gray; break;
                                    case '8': charColor = Color.DarkGray; break;
                                    case '9': charColor = Color.LightBlue; break;
                                    case 'a': charColor = Color.Lime; break;
                                    case 'b': charColor = Color.Cyan; break;
                                    case 'c': charColor = Color.Red; break;
                                    case 'd': charColor = Color.Magenta; break;
                                    case 'e': charColor = Color.Yellow; break;
                                    case 'f': charColor = Color.White; break;
                                    case 'o': charColor = Color.Orange; break;
                                    case 'n': charColor = Color.SaddleBrown; break;
                                    case 'r': charColor = Color.Gray; break;
                                }

                                if (parts[i].Length > 1)
                                {
                                    AppendColoredTextToConsole(parts[i].Substring(1, parts[i].Length - 1), charColor, false);
                                }
                            }
                        }
                    }
                    AppendColoredTextToConsole("", Color.White, true);
                    return false;
                });
                this.ShowAll();

            }
                
        }

        private void _gameCodeCopyButton_Click(object sender, EventArgs e)
        {
            if(!(_gameCodeEntryField.Text is null || _gameCodeEntryField.Text == ""))
            {
                _clipboard.Text = _gameCodeEntryField.Text;
            } 
           
        }
    
    }

}
