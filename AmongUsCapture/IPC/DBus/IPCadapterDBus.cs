using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedMemory;
using Tmds.DBus;
using Tmds.DBus.Transports;
using Color = System.Drawing.Color;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;


namespace AmongUsCapture.DBus
{
    class IPCadapterDBus : IPCadapter
    {
        private Thread _dbusProcessingThread;
        private CancellationTokenSource _cancellation;
        private IConnectLink _ipclink;

        public override URIStartResult HandleURIStart(string[] args)
        {
            var myProcessId = Process.GetCurrentProcess().Id;
            //Process[] processes = Process.GetProcessesByName("AmongUsCapture");
            //foreach (Process p in processes)
            //{
            //if (p.Id != myProcessId)
            //    {
            //        p.Kill();
            //    }
            // }
            Console.WriteLine(Program.GetExecutablePath());

            mutex = new Mutex(true, appName, out var createdNew);
            var wasURIStart = args.Length > 0 && args[0].StartsWith(UriScheme + "://");
            var result = URIStartResult.CONTINUE;

            if (!createdNew) // send it to already existing instance if applicable, then close
            {
                if (wasURIStart) SendToken(args[0]);

                return URIStartResult.CLOSE;
            }
            else if (wasURIStart) // URI start on new instance, continue as normal but also handle current argument
            {
                result = URIStartResult.PARSE;
            }

            RegisterProtocol();

            return result;
        }
        
        private static void RegisterProtocol()
        {
            
        }

        public override bool SendToken(string jsonText)
        {
            // Send the token via DBus.
            using (Connection conn = new Connection(Address.Session))
            {
                conn.ConnectAsync();
                
                var obj = new IPCLink();
                conn.RegisterObjectAsync(obj);
                conn.RegisterServiceAsync("org.amonguscapture", ServiceRegistrationOptions.None);

                obj.ConnectLink = jsonText;
            }

            return true;
        }

        public override void SendToken(string host, string connectCode)
        {
            var st = new StartToken {ConnectCode = connectCode, Host = host};
            OnTokenChanged(st);
        }

        public async override Task RegisterMinion()
        {
            using (var connection = new Connection(Address.Session))
            {
                await connection.ConnectAsync();
                    
                    _ipclink  = connection.CreateProxy<IConnectLink>("org.amonguscapture.linkipc",
                        "/AmongUsCapture/LinkIPC");

                    await _ipclink.WatchConnectInfoAsync(RespondToDbus);
            }
        }

        public override void startWithToken(string uri)
        {
            OnTokenChanged(StartToken.FromString(uri));
        }

        public override bool Cancel()
        {
            if (!_cancellation.IsCancellationRequested)
            { 
                _cancellation.Cancel();
                return true;
            }

            return false;
        }
        
        private void RespondToDbus(string signalresponse)
        {
            Settings.conInterface.WriteModuleTextColored("DBus", Color.Silver,
                $"$Recieved new message on DBus: {signalresponse}");

            OnTokenChanged(StartToken.FromString(signalresponse));
        }
    }
    
}