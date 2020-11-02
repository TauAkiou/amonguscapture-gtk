using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using AmongUsCapture.TextColorLibrary;
using SocketIOClient;

namespace AmongUsCapture
{
    public class ClientSocket
    {
        public event EventHandler<ConnectedEventArgs> OnConnected;
        public event EventHandler OnDisconnected;

        private SocketIO socket;
        private string ConnectCode;

        public void Init()
        {
            // Initialize a socket.io connection.
            socket = new SocketIO();

            // Handle tokens from protocol links.
            IPCadapter.getInstance().OnToken += async (s, e) => await OnTokenHandler(s, e);

                // Register handlers for game-state change events.
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
            GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;

            // Handle socket connection events.
            socket.OnConnected += (sender, e) =>
            {
                // Report the connection
                //Settings.form.setConnectionStatus(true);
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, "Connected successfully!");


                // Alert any listeners that the connection has occurred.
                OnConnected?.Invoke(this, new ConnectedEventArgs() {Uri = socket.ServerUri.ToString()});

                // On each (re)connection, send the connect code and then force-update everything.
                socket.EmitAsync("connectCode", ConnectCode).ContinueWith((_) =>
                {
                    Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                        $"Connection code ({Color.Red.ToTextColor()}{ConnectCode}) sent to server.");
                    GameMemReader.getInstance().ForceUpdatePlayers();
                    GameMemReader.getInstance().ForceTransmitState();
                    GameMemReader.getInstance().ForceTransmitLobby();
                });
            };

            // Handle socket disconnection events.
            socket.OnDisconnected += (sender, e) =>
            {
                //Settings.form.setConnectionStatus(false);
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                    $"{Color.Red.ToTextColorPango("Connection lost!")}");

                // Alert any listeners that the disconnection has occured.
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            };
        }

        public async Task OnTokenHandler(object sender, StartToken token)
        {
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Attempting to connect to host {Color.LimeGreen.ToTextColorPango(token.Host)} with connect code {Color.Red.ToTextColorPango(token.ConnectCode)}");
            if (socket.Connected)
                // Disconnect from the existing host...
                await socket.DisconnectAsync().ContinueWith(async (t) =>
                {
                    // ...then connect to the new one.
                    await Connect(token.Host, token.ConnectCode);
                });
            else
                // Connect using the host and connect code specified by the token.
                await Connect(token.Host, token.ConnectCode);
        }

        private void OnConnectionFailure(AggregateException e = null)
        {
            var message = e != null ? e.Message : "A generic connection error occured.";
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"{Color.Red.ToTextColorPango(message)}");
        }

        private async Task Connect(string url, string connectCode)
        {
            try
            {
                ConnectCode = connectCode;
                socket.ServerUri = new Uri(url);
                if (socket.Connected) await socket.DisconnectAsync();
                await socket.ConnectAsync().ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        OnConnectionFailure(t.Exception);
                        return;
                    }
                });
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Invalid bot host, not connecting");
            }
            catch (UriFormatException)
            {
                Console.WriteLine("Invalid bot host, not connecting");
            }
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("state",
                JsonSerializer
                    .Serialize(e.NewState)); // could possibly use continueWith() w/ callback if result is needed
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("player",
                JsonSerializer.Serialize(e)); //Makes code wait for socket to emit before closing thread.
        }

        private void JoinedLobbyHandler(object sender, LobbyEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("lobby", JsonSerializer.Serialize(e));
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Room code ({Color.Yellow.ToTextColorPango(e.LobbyCode)}) sent to server.");
        }
    }

    public class ConnectedEventArgs : EventArgs
        {
            public string Uri { get; set; }
        }
    }