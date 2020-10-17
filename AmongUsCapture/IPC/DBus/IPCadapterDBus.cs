using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Gtk;
using Mono.Unix;
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
        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private Connection _dbusconnection;
        private IConnectLink _ipclink;
        private bool _isListening;

        public override URIStartResult HandleURIStart(string[] args)
        {
            var myProcessId = Process.GetCurrentProcess().Id;
            //Process[] processes = Process.GetProcessesByName("AmongUsCapture");
            //Process[] dotnetprocesses = Process.GetProcessesByName("dotnet");
            //foreach (Process p in processes)
            //{
            //if (p.Id != myProcessId)
            //    {
            //        p.Kill();
            //    }
            // }
            Console.WriteLine(Program.GetExecutablePath());

            //mutex = new Mutex(true, appName, out var createdNew);
            bool createdNew = false;
            var wasURIStart = args.Length > 0 && args[0].StartsWith(UriScheme + "://");
            var result = URIStartResult.CONTINUE;

            if (!File.Exists(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
            {
                createdNew = true;
            }
            else
            {
                // Open our PID file.
                using (var pidfile = File.OpenText(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
                {
                    var pid = pidfile.ReadLine();
                    if (pid != null)
                    {
                        var pidint = Int32.Parse(pid);

                        try
                        {
                            var capproc = Process.GetProcessById(pidint);
                            var assmbname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                            var runnername = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                            if (!capproc.ProcessName.Contains("dotnet"))
                            {
                                // We're just going to assume that a dotnet process that matches this set of parameters
                                // is a running capture process.
                                throw new ArgumentException();

                            }
                            else if (!capproc.ProcessName.Contains(Path.GetFileName(runnername)))
                            {
                                throw new ArgumentException();
                            }


                            if (capproc.HasExited)
                            {
                                throw new ArgumentException();
                            }
                        }
                        catch (ArgumentException e)
                        {
                            // Process doesn't exist. Clear the file.
                            Console.WriteLine($"Found stale PID file containing {pid}.");
                            File.Delete(Path.Join(Settings.StorageLocation, ".amonguscapture.pid"));
                            createdNew = true;
                        }
                    }
                }

            }

            if (createdNew)
            {
                using (var pidwriter = File.CreateText(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
                {
                    pidwriter.Write(myProcessId);
                }
            }
            

            if (!createdNew) // send it to already existing instance if applicable, then close
            {
                if (wasURIStart) SendToken(args[0]).Wait();

                return URIStartResult.CLOSE;
            }
            else if (wasURIStart) // URI start on new instance, continue as normal but also handle current argument
            {
                // if we are running, we create a file lock with our process in it.
                // Also attach the pid delete handler from Program.

                result = URIStartResult.PARSE;
            }

            RegisterProtocol();

            return result;
        }

        private static void RegisterProtocol()
        {
            // we really should query the user for this, but since Dialogs appear to be completely fucked, we're going
            // to just install it right now.
            var xdg_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "applications");
            var xdg_file = Path.Join(xdg_path, "aucapture-opener.desktop");

            if (!File.Exists(xdg_file))
            {
                var executingassmb = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (Path.HasExtension("dll"))
                {
                    executingassmb = "dotnet " + executingassmb;
                }

                var xdg_file_write = new string[]
                {
                    "[Desktop Entry]",
                    "Type=Application",
                    "Name=aucapture URI Handler",
                    $"Exec={executingassmb} %u",
                    "StartupNotify=false",
                    "MimeType=x-scheme-handler/aucapture;"
                };

                using (var file = File.CreateText(xdg_file))
                {
                    foreach (string str in xdg_file_write)
                    {
                        file.WriteLine(str);
                    }
                }

                var xdg_posix = new UnixFileInfo(xdg_file);

                xdg_posix.FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute
                                                  | FileAccessPermissions.GroupRead
                                                  | FileAccessPermissions.GroupExecute
                                                  | FileAccessPermissions.OtherRead
                                                  | FileAccessPermissions.OtherExecute;

                // Finally, register with xdg-mime.

                var xdgproc = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/xdg-mime",
                        Arguments = $"default aucapture-opener.desktop x-scheme-handler/aucapture",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                xdgproc.Start();
                string result = xdgproc.StandardOutput.ReadToEnd();
                xdgproc.WaitForExit();
            }
        }

        public override async Task<bool> SendToken(string jsonText)
        {
            // Send the token via DBus.
            using (Connection conn = new Connection(Address.Session))
            {
                await conn.ConnectAsync();

                var obj = new IPCLink();
                await conn.RegisterObjectAsync(obj);
                await conn.RegisterServiceAsync("org.AmongUsCapture.ipc", ServiceRegistrationOptions.None);

                obj.ConnectLink = jsonText;
            }

            return true;
        }

        public override void SendToken(string host, string connectCode)
        {
            var st = new StartToken {ConnectCode = connectCode, Host = host};
            OnTokenEvent(st);
        }

        public async override Task RegisterMinion()
        {
            Task.Factory.StartNew(async () =>
            {

                using (_dbusconnection = new Connection(Address.Session))
                {
                    await _dbusconnection.ConnectAsync();

                    _ipclink = _dbusconnection.CreateProxy<IConnectLink>("org.AmongUsCapture.ConnectLink",
                        "/org/AmongUsCapture/ConnectLink");

                    await _ipclink.WatchConnectInfoAsync(RespondToDbus);

                    _isListening = true;

                    while (!_cancellation.IsCancellationRequested)
                    {
                        _cancellation.Token.ThrowIfCancellationRequested();
                        await Task.Delay(int.MaxValue);
                    }
                }
            });

        }

        public override void startWithToken(string uri)
        {
            OnTokenEvent(StartToken.FromString(uri));
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
                $"Received new message on DBus: \"{signalresponse}\"");

            signalresponse = signalresponse.Trim('\r', '\n');

            var token = StartToken.FromString(signalresponse);

            OnTokenEvent(token);
        }
    }
}