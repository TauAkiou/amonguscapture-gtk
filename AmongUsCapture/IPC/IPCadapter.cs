using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AmongUsCapture.DBus;
using AmongUsCapture.Windows;
using Gtk;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharedMemory;

namespace AmongUsCapture
{
    abstract class IPCadapter
    {
        public const string appName = "AmongUsCapture";
        protected const string UriScheme = "aucapture";
        protected const string FriendlyName = "AmongUs Capture";
        protected Mutex mutex;
        


        
        private static IPCadapter instance;
        public static IPCadapter getInstance()
        {
            if (instance == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Use the RPC Buffer for Windows.
                    instance = new IPCadapterRpcBuffer();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Use DBus for Linux.
                    instance = new IPCadapterDBus();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return instance;
            
        }
        
        public event EventHandler<StartToken> OnToken;
        protected virtual void OnTokenEvent(StartToken e)
        {
            // Safely raise the event for all subscribers
            OnToken?.Invoke(this, e);
        }

        public abstract URIStartResult HandleURIStart(string[] args);
        public abstract Task<bool> SendToken(string jsonText);
        public abstract void SendToken(string host, string connectCode);
        public abstract Task RegisterMinion();
        public abstract void startWithToken(string uri);

        // This method is for implementations that might be cancelable - such as DBUS.
        public virtual bool Cancel()
        {
            return true;
        }
    }

    public enum URIStartResult
    {
        CLOSE,
        PARSE,
        CONTINUE
    }

    public class StartToken : EventArgs
    {
        public string Host { get; set; }
        public string ConnectCode { get; set; }

        public static StartToken FromString(string rawToken)
        {
            try
            {
                rawToken = new string(rawToken.Where(c => !char.IsControl(c)).ToArray());
                Uri uri = new Uri(rawToken);
                NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
                bool insecure = (nvc["insecure"] != null && nvc["insecure"] != "false") || uri.Query == "?insecure";
                return new StartToken() { Host = (insecure ? "http://" : "https://") + uri.Authority, ConnectCode = uri.AbsolutePath.Substring(1) };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StartToken();
            }
        }
    }

}
