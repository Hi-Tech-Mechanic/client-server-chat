namespace Server
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.Json;

    public static class ServerModel
    {
        private const string dataFile = "server_data.json";
        internal static readonly List<TcpClient> Clients = new();
        public static ObservableCollection<TcpListener> TcpListeners { get; private set; } = new();

        public static bool IsAlive { get; private set; } = false;

        public static event Action ServersCountChanged = null!;

        static ServerModel()
        {
            IsAlive = true;

            TcpListeners = LoadServers();
            TcpListeners.CollectionChanged += OnItemsChangedHandler;
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

        #region PublicMethods

        public static void AddServer(TcpListener tcpListener)
        {
            tcpListener.Start();
            TcpListeners.Add(tcpListener);
        }

        public static void RemoveServer(TcpListener tcpListener)
        {
            TcpListeners.Remove(tcpListener);
        }

        public static void ClearAllServers()
        {
            TcpListeners.Clear();
        }

        #endregion

        #region LocalMethods

        private static void OnItemsChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ServersCountChanged?.Invoke();
            SaveServers();

            if (e.OldItems != null)
            {
                if (ListenersIsEmpty())
                {
                    IsAlive = false;

                    if (File.Exists(dataFile))
                        File.Delete(dataFile);
                }
            }

            static bool ListenersIsEmpty()
            {
                return TcpListeners?.Count == 0;
            }
        }

        #endregion

        #region Save&Load

        private static void SaveServers()
        {
            var tmpTcpListeners = TcpListeners;
            List<TcpListenerInfo> currentServers = new();
            foreach (var listener in tmpTcpListeners)
            {
                if (listener.LocalEndpoint is IPEndPoint endPoint)
                {
                    currentServers.Add(new TcpListenerInfo
                    {
                        IpAddress = endPoint.Address.ToString(),
                        Port = endPoint.Port,
                        Active = (listener.Server != null || listener.Server!.IsBound),
                    });
                }
            }

            //var previousData = LoadTcpListenerInfo();
            //List<TcpListenerInfo> combinedData = previousData.Concat(currentServers).ToList();
            string newData = JsonSerializer.Serialize(currentServers);
            //newData.Distinct();
            File.WriteAllText(dataFile, newData);
        }

        public static ObservableCollection<TcpListener> LoadServers()
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
