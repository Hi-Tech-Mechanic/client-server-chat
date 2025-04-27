namespace Server
{
    using System.Net;
    using System.Net.Sockets;
    using System.IO;
    using System.Text.Json;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using Data;

    public static class ServerModel
    {
        private const string dataFile = "server_data.json";
        internal static readonly List<TcpClient> Clients = new List<TcpClient>();
        //public static ObservableCollection<User> Users { get; private set; } = new();
        //public static ObservableCollection<ChatForm> ChatForm = new();
        public static ObservableCollection<TcpListener> TcpListeners { get; private set; } = new();
        public static bool IsAlive { get; private set; } = false;

        public static event Action ServersCountChanged = null!;

        static ServerModel()
        {
            IsAlive = true;
            TcpListeners.CollectionChanged += OnItemsChangedHandler;

            TcpListeners = LoadServers();
        }

        public static int GetPort(int serverIndex)
        {
            int port;

            if (TcpListeners[serverIndex].Server != null && TcpListeners[serverIndex].Server.LocalEndPoint != null)
            {
                port = ((IPEndPoint)TcpListeners[serverIndex].Server?.LocalEndPoint!).Port;
            }
            else
            {
                port = LoadTcpListenerInfo()[serverIndex].Port;
            }

            return port;
        }

        public static EndPoint? GetLocalEndPoint(int serverIndex)
        {
            var endPoint = TcpListeners[serverIndex].Server.LocalEndPoint;
            return endPoint;
        }

        #region LocalMethods

        internal static void AddServer(TcpListener tcpListener)
        {
            tcpListener.Start();
            TcpListeners.Add(tcpListener);

            SaveServers(TcpListeners);
        }

        internal static void RemoveServer(TcpListener tcpListener)
        {
            TcpListeners.Remove(tcpListener);
            SaveServers(TcpListeners);
        }

        private static void OnItemsChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ServersCountChanged?.Invoke();

            if (e.OldItems != null)
            {
                if (ListenersIsEmpty())
                {
                    IsAlive = false;

                    if (File.Exists(dataFile))
                        File.Delete(dataFile);
                }
            }

            bool ListenersIsEmpty()
            {
                return TcpListeners.Count == 0;
            }
        }

        #endregion

        #region Save&Load

        private static void SaveServers(ObservableCollection<TcpListener> tcpListeners)
        {
            List<TcpListenerInfo> currentServers = new();
            foreach (var listener in tcpListeners)
            {
                var endPoint = listener.LocalEndpoint as IPEndPoint;
                if (endPoint != null)
                {
                    currentServers.Add(new TcpListenerInfo
                    {
                        IpAddress = endPoint.Address.ToString(),
                        Port = endPoint.Port,
                        Active = (listener.Server != null || listener.Server!.IsBound) ? true : false,
                    });
                }
            }

            var previousData = LoadTcpListenerInfo();
            List<TcpListenerInfo> combinedData = previousData.Concat(currentServers).ToList();
            string newData = JsonSerializer.Serialize(combinedData);
            File.WriteAllText(dataFile, newData);
        }

        private static ObservableCollection<TcpListener> LoadServers()
        {
            if (File.Exists(dataFile) == false)
                return new ObservableCollection<TcpListener>();

            var listenerInfos = LoadTcpListenerInfo();
            var tcpListeners = new ObservableCollection<TcpListener>();

            foreach (var info in listenerInfos)
            {
                var listener = new TcpListener(IPAddress.Parse(info.IpAddress), info.Port);
                if (info.Active == false)
                {
                    listener.Start();
                }
                tcpListeners.Add(listener);
            }

            return tcpListeners;
        }

        private static List<TcpListenerInfo> LoadTcpListenerInfo()
        {
            if (File.Exists(dataFile) == false)
                return new List<TcpListenerInfo>();

            string json = File.ReadAllText(dataFile);
            var listenerInfos = JsonSerializer.Deserialize<List<TcpListenerInfo>>(json);
            if (listenerInfos == null)
                throw new NullReferenceException($"Не найден сохраненный файл: {json}");

            return listenerInfos;
        }

        #endregion

        public class TcpListenerInfo
        {
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; }
            public bool Active { get; set; }
        }
    }
}
