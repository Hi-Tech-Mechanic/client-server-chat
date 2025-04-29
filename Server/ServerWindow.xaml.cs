namespace Server
{
    using Helpers;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Windows;
    
    /// <summary>
    /// Логика взаимодействия для ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private readonly TcpListener currentTcpListener;

        public ServerWindow()
        {
            this.InitializeComponent();
            this.InitializeServer();
            this.currentTcpListener = ServerModel.TcpListeners.Last();
            this.StartServer();

            this.Closing += this.Dispose;
        }

        private void InitializeServer()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 0); // Автоматический выбор свободных портов
            ServerModel.AddServer(server);
        }

        private async void StartServer()
        {
            try
            {
                var lastServer = ServerModel.TcpListeners.Count - 1;
                var logMessage = $"Сервер запущен на порту {ServerModel.GetPort(lastServer)}\n";
                this.DisplayInServerFormAndConsole(logMessage);

                while (true)
                {
                    TcpClient client = await currentTcpListener.AcceptTcpClientAsync();
                    ServerModel.Clients.Add(client);

                    logMessage = $"Клиент подключился: {client.Client.RemoteEndPoint}";
                    this.DisplayInServerFormAndConsole(logMessage);

                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Серверная ошибка: {ex}");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int byteRead = await stream.ReadAsync(buffer);
                    if (byteRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, byteRead);
                    string logMessage = $"Получено от {client.Client.RemoteEndPoint}: {message}";
                    this.DisplayInServerFormAndConsole(logMessage);

                    await this.BroadcastMessageAsync(message, client);
                }
            }
            catch (Exception ex)
            {
                var logMessage = $"Ошибка обработки клиента {client.Client.RemoteEndPoint}: {ex.Message}";
                this.DisplayInServerFormAndConsole(logMessage);
            }
            finally
            {
                ServerModel.Clients.Remove(client);

                var logMessage = $"Пользователь отключился: {client.Client.RemoteEndPoint}";
                this.DisplayInServerFormAndConsole(logMessage);

                client.Close();
            }
        }

        private async Task BroadcastMessageAsync(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            var localClients = ServerModel.Clients;

            foreach (TcpClient client in localClients)
            {
                if (client != sender)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        await stream.WriteAsync(data);

                        var logMessage = $"Отправлено к {client.Client.RemoteEndPoint}: {message}";
                        this.DisplayInServerFormAndConsole(logMessage);
                    }
                    catch (Exception ex)
                    {
                        var logMessage = $"Ошибка отправки к {client.Client.RemoteEndPoint}: {ex.Message}";
                        this.DisplayInServerFormAndConsole(logMessage);
                    }
                }
            }
        }

        private void Dispose(object? sender, CancelEventArgs e)
        {
            ServerModel.RemoveServer(currentTcpListener);
            this.Closing -= Dispose;
        }

        private void DisplayInServerFormAndConsole(string? message)
        {
            LoggingHelper.AddMessageToTextBox(this.ServerTextBox, message);
            LoggingHelper.WriteLineToConsole(message);
        }
    }
}
