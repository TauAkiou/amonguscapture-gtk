using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace AmongUsCapture.DBus
{
    [DBusInterface("org.amonguscapture")]
    public interface IConnectLink : IDBusObject
    {
        Task<IDisposable> WatchConnectInfoAsync(Action<string> handler);
    }
    
    public class IPCLink : IConnectLink
    {
        string _connectlink;
        public string ConnectLink
        {
            get { return _connectlink; }
            set 
            {
                _connectlink = value;
                SentLink?.Invoke(_connectlink);
            }
        }
        public ObjectPath ObjectPath => new ObjectPath("/org/amonguscapture/ipclink");
        public event Action<string> SentLink;
        
        public Task<IDisposable> WatchConnectInfoAsync(Action<string> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(SentLink), handler);
        }
    }
}