using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gdk;
using GLib;
using Gtk;
using Color = System.Drawing.Color;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;
using Window = Gtk.Window;
using AmongUsCapture.TextColorLibrary;
using Object = Atk.Object;

namespace AmongUsCapture
{
    public partial class UserForm : Window
    {
        private bool _autoscroll = false;
        
        private ClientSocket clientSocket;
        private static Atom _atom = Atom.Intern("CLIPBOARD", false);
        private Clipboard _clipboard = Clipboard.Get(_atom);
        private LobbyEventArgs lastJoinedLobby;
        public static Color NormalTextColor = Color.Black;
        private static object locker = new object();
        private Queue<string> deadMessageQueue = new Queue<string>();

        
        
        private Color Rainbow(float progress)
        {
            var div = Math.Abs(progress % 1) * 6;
            var ascending = (int) (div % 1 * 255);
            var descending = 255 - ascending;

            switch ((int) div)
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
      
        public UserForm(ClientSocket sock) : base("Among Us Capture - GTK")
        {
            //builder.Autoconnect(this);
            Icon = new Pixbuf(Assembly.GetExecutingAssembly().GetManifestResourceStream("amonguscapture_gtk.icon.ico"));
            clientSocket = sock;
            InitializeWindow();
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += UserForm_PlayerChanged;
            GameMemReader.getInstance().ChatMessageAdded += OnChatMessageAdded;
            GameMemReader.getInstance().JoinedLobby += OnJoinedLobby;
            GameMemReader.getInstance().GameVersionUnverified += _eventGameIsUnverified;
            
            // Load URL
            _urlHostEntryField.Text = Settings.PersistentSettings.host;

            // Connect on Enter
            //this.AcceptButton = ConnectButton;
            this.Default = _connectCodeSubmitButton;

            // Get the user's default GTK TextView foreground color.
            NormalTextColor = GetRgbColorFromFloat(_consoleTextView.StyleContext.GetColor(Gtk.StateFlags.Normal));

        }

        private void _eventGameIsUnverified(object o, ValidatorEventArgs e)
        {
            Gtk.Application.Invoke((obj, ev) =>
            {
                var badversionBox = new MessageDialog(this,
                    DialogFlags.Modal,
                    MessageType.Warning,
                    ButtonsType.None,
                    false,
                    "We have detected an unverified version of Among Us. The capture may not work properly.",
                new object[] { });

                if (e.Validity.HasFlag(AmongUsValidity.GAME_VERIFICATION_FAIL))
                {
                    badversionBox.Text += "\n\nThis version of Among Us appears to be an out-of-date or Beta version of the game.";
                }
                
                var marea = badversionBox.MessageArea as Box;

                if (e.Validity.HasFlag(AmongUsValidity.STEAM_VERIFICAITON_FAIL))
                {
                    badversionBox.Text +=
                        "\n\nThis version appears to be a cracked or pirated version of the game. Please consider buying a copy of the game at the link below.";
                    marea.Add(new LinkButton("https://store.steampowered.com/app/945360/Among_Us/", "Open Steam Store"));
                }

                badversionBox.Text += "\n\nWe cannot provide support for this configuration should you choose to continue.";
                
                badversionBox.AddButton("Quit", ResponseType.Reject);
                badversionBox.AddButton("I Understand", ResponseType.Accept);
                
                badversionBox.Response += delegate(object o1, ResponseArgs args)
                {
                    if (args.ResponseId == ResponseType.Reject)
                    {
                        Close();
                    }

                    if (args.ResponseId == ResponseType.Accept)
                    {
                        GameMemReader.getInstance().cracked = false;
                        GameMemReader.getInstance().invalidversion = false;
                        GameMemReader.getInstance().paused = false;
                    }
                };
                
                badversionBox.ShowAll();
                badversionBox.Run();
                badversionBox.Dispose();
            });


            Settings.conInterface.WriteModuleTextColored("Notification", Color.Red,
                $"We have detected an unverified version of Among Us. Things may not work properly.");
        }
        

        private void _primaryWindowMenuQuitItem_Activated(object o, EventArgs e)
        {
            this.Close();
        }

        private void _primaryWindowMenuItemAbout_Activated(object o, EventArgs e)
        {
            var abouticon = new Pixbuf(Assembly.GetExecutingAssembly().GetManifestResourceStream("amonguscapture_gtk.icon.ico"));
            string version = String.Empty;
            string master = String.Empty;
            string license = String.Empty;
            List<String> contributorlist = new List<string>();
            
            using(Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("amonguscapture_gtk.version.txt"))
                if (stream == null)
                    version = "Unknown";
                else
                {
                    using (StreamReader sreader = new StreamReader(stream))
                    {
                        version = sreader.ReadToEnd();
                    }
                }
            
            using(Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("amonguscapture_gtk.master.txt"))
            {    
                // Contains the original tag/hash from the source build.
                using (StreamReader sreader = new StreamReader(stream))
                {
                    master = sreader.ReadToEnd();
                }
            }
            
            using(Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("amonguscapture_gtk.contributors.txt"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string contrib;
                    while ((contrib = reader.ReadLine()) != null)
                    {
                        contributorlist.Add(contrib);
                    }
                }
            }

            AboutDialog about = new AboutDialog()
            {
                Name = "_amonguscaptureGtkAboutDialog",
                ProgramName = "Among Us Capture GTK",
                LicenseType = License.MitX11,
                Icon = abouticon,
                Version = version,
                Authors = contributorlist.ToArray(),
                Comments = "Capture of the local Among Us executable state, cross-platform rewrite in GTK." +
                $"\n\nBased on amonguscapture {master}",
                Website = "https://github.com/TauAkiou/amonguscapture-gtk",
                Logo = abouticon
            };

            about.Present();
            about.Run();
            
            // Make sure the About dialog box is cleaned up.
            about.Dispose();
        }

        private void _consoleTextView_OnPopulateContextMenu(object o, PopulatePopupArgs e)
        {
            Menu textViewContextMenu = (Menu)e.Args[0];
            SeparatorMenuItem _contextMenuSeperator = new SeparatorMenuItem();

            CheckMenuItem _autoscrollMenuItem = new CheckMenuItem()
            {
                Name = "_autoscrollMenuItem",
                Label = "Auto Scroll",
                TooltipText = "Enable or disable console autoscrolling",
                Active = _autoscroll
            };
            
            _autoscrollMenuItem.Toggled += delegate(object sender, EventArgs args)
            {
                // it has to be written this way to get around a crash.
                // don't know why, but i do what must be done.
                var button = sender as CheckMenuItem;
                _autoscroll = button.Active;
            };

            textViewContextMenu.Append(_contextMenuSeperator);
            textViewContextMenu.Append(_autoscrollMenuItem);
            textViewContextMenu.ShowAll();
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
            //TestFillConsole(1000);
        }

        private string getRainbowText(string nonRainbow, int shift = 0)
        {
            var OutputString = "";
            for (var i = 0; i < nonRainbow.Length; i++)
                OutputString += Rainbow((float) ((i + shift) % nonRainbow.Length) / nonRainbow.Length).ToTextColor() +
                                nonRainbow[i];
            return OutputString;
        }

        private void OnChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColorPango(e.Sender)}{e.Message}");
        }

        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            if (e.Action == PlayerAction.Died)
                deadMessageQueue.Enqueue(
                    $"{PlayerColorToColorOBJ(e.Color).ToTextColorPango(e.Name)}: {e.Action}");
            else
                Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki,
                    $"{PlayerColorToColorOBJ(e.Color).ToTextColorPango(e.Name)}: {e.Action}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }
        
        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            while (deadMessageQueue.Count > 0) //Lets print out the state changes now that gamestate has changed.
            {
                var text = deadMessageQueue.Dequeue();
                Settings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, text);
            }
            
            Idle.Add(delegate
            {
                _currentStateLabel.Text = e.NewState.ToString();
                return false;
            });
            Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"State changed to {Color.Cyan.ToTextColorPango(e.NewState.ToString())}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }

        private void _connectCodeSubmitButton_Click(object sender, EventArgs e)
        {

            _connectCodeEntryField.Sensitive = false;
            _connectCodeSubmitButton.Sensitive = false;
            _urlHostEntryField.Sensitive = false;

            var url = "http://localhost:8123";
            if (_urlHostEntryField.Text != "")
            {
                url = _urlHostEntryField.Text;
            }

            doConnect(url);
        }
        
        private void doConnect(string url)
        {
            try
            {
                clientSocket.OnTokenHandler(null,
                    new StartToken() {Host = url, ConnectCode = _connectCodeEntryField.Text});
            }
            catch (Exception e)
            {
                // TODO: Add GTK code for error box here
                Gtk.Application.Invoke(delegate(object? sender, EventArgs args)
                {
                    var errorbox = new MessageDialog(this,
                        DialogFlags.UseHeaderBar,
                        MessageType.Error,
                        ButtonsType.Close,
                        e.Message);

                    errorbox.ShowAll();
                    errorbox.Run();
                    errorbox.Dispose();
                });
            }
            finally
            {
                _connectCodeEntryField.Sensitive = true;
                _connectCodeSubmitButton.Sensitive = true;
                _urlHostEntryField.Sensitive = true;
            }
        }

        /*
        private void ConnectCodeBox_TextChanged(object sender, EventArgs e)
        {
            ConnectButton.Enabled = (ConnectCodeBox.Enabled && ConnectCodeBox.Text.Length == 8 && ConnectCodeBox.MaskCompleted);
        }
        */
        
        private void _consoleTextView_BufferChanged(object sender, EventArgs e)
        { 
            if (_autoscroll)
            {
                var scrolladj = _consoleScrolledWindow.Vadjustment;
                scrolladj.Value = scrolladj.Upper - scrolladj.PageSize;
            }
            
        }

        private void TestFillConsole(int entries) //Helper test method to see if filling console works.
        {
            for (var i = 0; i < entries; i++)
            {
                var nonString = "Wow! Look at this pretty text!";
                Settings.conInterface.WriteModuleTextColored("Rainbow", Rainbow((float) i / entries),
                    getRainbowText(nonString, i));
            }

            ;
            //this.WriteColoredText(getRainbowText("This is a Pre-Release from Carbon's branch."));
        }

        public void WriteConsoleLineFormatted(string moduleName, Color moduleColor, string message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            WriteColoredText(
                $"{NormalTextColor.ToTextColor()}[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
        }

        public void WriteColoredText(string ColoredText)
        {
            lock (locker)
            {
                foreach (var part in TextColor.toParts(ColoredText))
                    AppendColoredTextToConsole(part.text, part.textColor);
                AppendColoredTextToConsole("", Color.White, true);
            }
            //autoscroll();
        }

        public void AppendColoredTextToConsole(string line, Color color, bool addNewLine = false)
        {
            if (!(_consoleTextView is null))
            {
                Idle.Add(delegate
                {
                    var iter = _consoleTextView.Buffer.EndIter;
                    _consoleTextView.Buffer.InsertMarkup(ref iter, addNewLine
                        ? $"<span foreground=\"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}\">{line}</span>{Environment.NewLine}" 
                        : $"<span foreground=\"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}\">{line}</span>");
                    _consoleTextView.Buffer.PlaceCursor(iter);
                return false;
                });
            }
        }

        public void WriteLineToConsole(string line)
        {
            if (!(_consoleTextView is null))
            {
                lock (locker)
                {
                    Idle.Add(delegate
                    {
                        var iter = _consoleTextView.Buffer.EndIter;
                        _consoleTextView.Buffer.Insert(ref iter,line + "\n");
                        _consoleTextView.Buffer.PlaceCursor(iter);
                        return false;
                    });
                }

                //autoscroll();
            }
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
            var OutputCode = "";
            switch (pColor)
            {
                case PlayerColor.Red:
                    OutputCode = "§c";
                    break;
                case PlayerColor.Blue:
                    OutputCode = "§1";
                    break;
                case PlayerColor.Green:
                    OutputCode = "§2";
                    break;
                case PlayerColor.Pink:
                    OutputCode = "§d";
                    break;
                case PlayerColor.Orange:
                    OutputCode = "§o";
                    break;
                case PlayerColor.Yellow:
                    OutputCode = "§e";
                    break;
                case PlayerColor.Black:
                    OutputCode = "§0";
                    break;
                case PlayerColor.White:
                    OutputCode = "§f";
                    break;
                case PlayerColor.Purple:
                    OutputCode = "§5";
                    break;
                case PlayerColor.Brown:
                    OutputCode = "§n";
                    break;
                case PlayerColor.Cyan:
                    OutputCode = "§b";
                    break;
                case PlayerColor.Lime:
                    OutputCode = "§a";
                    break;
            }

            return OutputCode;
        }

        public void WriteLineFormatted(string str, bool acceptnewlines = true)
        {
            if (!(_consoleTextView is null))
            {
                Idle.Add(delegate
                {
                    lock (locker)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (!acceptnewlines) str = str.Replace('\n', ' ');
                            var parts = str.Split(new[] {'§'});
                            if (parts[0].Length > 0) AppendColoredTextToConsole(parts[0], Color.White);
                            for (var i = 1; i < parts.Length; i++)
                            {
                                var charColor = Color.White;
                                if (parts[i].Length > 0)
                                {
                                    switch (parts[i][0])
                                    {
                                        case '0':
                                            charColor = Color.Gray;
                                            break; //Should be Black but Black is non-readable on a black background
                                        case '1':
                                            charColor = Color.RoyalBlue;
                                            break;
                                        case '2':
                                            charColor = Color.Green;
                                            break;
                                        case '3':
                                            charColor = Color.DarkCyan;
                                            break;
                                        case '4':
                                            charColor = Color.DarkRed;
                                            break;
                                        case '5':
                                            charColor = Color.MediumPurple;
                                            break;
                                        case '6':
                                            charColor = Color.DarkKhaki;
                                            break;
                                        case '7':
                                            charColor = Color.Gray;
                                            break;
                                        case '8':
                                            charColor = Color.DarkGray;
                                            break;
                                        case '9':
                                            charColor = Color.LightBlue;
                                            break;
                                        case 'a':
                                            charColor = Color.Lime;
                                            break;
                                        case 'b':
                                            charColor = Color.Cyan;
                                            break;
                                        case 'c':
                                            charColor = Color.Red;
                                            break;
                                        case 'd':
                                            charColor = Color.Magenta;
                                            break;
                                        case 'e':
                                            charColor = Color.Yellow;
                                            break;
                                        case 'f':
                                            charColor = Color.White;
                                            break;
                                        case 'o':
                                            charColor = Color.Orange;
                                            break;
                                        case 'n':
                                            charColor = Color.SaddleBrown;
                                            break;
                                        case 'r':
                                            charColor = Color.Gray;
                                            break;
                                    }

                                    if (parts[i].Length > 1)
                                        AppendColoredTextToConsole(parts[i].Substring(1, parts[i].Length - 1),
                                            charColor);
                                }
                            }
                        }

                        AppendColoredTextToConsole("", Color.White, true);
                        return false;
                    }

                    //autoscroll();
                });
            }
        }

        public void ShowCrackedBox()
        {   /*
            var result =
                MessageBox.Show("You are running a cracked version of Among Us. We do not support piracy.",
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            */
        }

        private void _gameCodeCopyButton_Click(object sender, EventArgs e)
        {
            if(!(_gameCodeEntryField.Text is null || _gameCodeEntryField.Text == ""))
            {
                _clipboard.Text = _gameCodeEntryField.Text;
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