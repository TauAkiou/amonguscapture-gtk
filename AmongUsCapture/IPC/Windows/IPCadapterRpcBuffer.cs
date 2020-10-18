using System;
using System.Text;
using System.Threading.Tasks;
using SharedMemory;
using Mutex = System.Threading.Mutex;
using Process = System.Diagnostics.Process;
#pragma warning disable 4014

namespace AmongUsCapture.Windows
{
    class IPCadapterRpcBuffer : IPCadapter
    {
        
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

        public override void SendToken(string host, string connectCode)
        {
            var st = new StartToken {ConnectCode = connectCode, Host = host};
            OnTokenEvent(st);
        }

        public async override Task<bool> SendToken(string jsonText)
        {
            var rpcGru = new RpcBuffer(appName); //Soup told me not to but its funny
            var RPCresult = rpcGru.RemoteRequest(Encoding.UTF8.GetBytes(jsonText));
            var MinionResponse = Encoding.UTF8.GetString(RPCresult.Data, 0, RPCresult.Data.Length);
            return RPCresult.Success;
        }

        private static void RegisterProtocol()
        {
            // Literally code that only works under Windows. 
            #if _WINDOWS
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
                // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
                var applicationLocation = Program.GetExecutablePath();

                key.SetValue("", "URL:" + FriendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
            #endif
        }

        public override Task RegisterMinion()
        {
            var rpcMinion = new RpcBuffer(appName, (msgId, payload) =>
            {
                var serverResponse = "Carbon has a huge pp also this is debug messages.";
                var gotData = Encoding.UTF8.GetString(payload, 0, payload.Length);
                Console.WriteLine($"RPCMinion: Got data: {gotData}");
                OnTokenEvent(StartToken.FromString(gotData));; //Invoke method and return.
                return Encoding.UTF8.GetBytes(serverResponse);
            });
            return Task.CompletedTask;
        }

        public override void startWithToken(string uri)
        {
            OnTokenEvent(StartToken.FromString(uri));;
        }
    }
}