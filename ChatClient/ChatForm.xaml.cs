namespace ChatClient
{
    using System.Net.Sockets;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using Helpers;
    using Data;
    using Server;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ChatForm : Window
    {
        /// <summary>
        /// Измерение в байтах
        /// </summary>
        private const ushort bufferSize = 1024;

        private List<User> Users; // warn возмонж не надо использовать список, так как в одном экземпляре используется 1 пользователь
        private NetworkStream stream;

        private string serverIdentifier { get; }

        public ChatForm(List<User> users, string ServerIdentifier)
        {
            InitializeComponent();

            this.Users = users;
            this.serverIdentifier = ServerIdentifier;
            this.Loaded += DisplayServerName;
            ServerWindow.OnSendMessage += AddMessageToChat;

            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                var user = this.Users.Last();
                await user.TcpClient.ConnectAsync("localhost", user.UsedServerPort);
                stream = user.TcpClient.GetStream();
                this.AddMessageToChat($"{user.UserName} - подключен к серверу");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                this.AddMessageToChat($"Ошибка подключения к серверу: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[bufferSize];

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Dispatcher.Invoke(() =>
                    {
                        this.AddMessageToChat($"[{Users.Last().UserName}]{message}"); // todo
                    });
                }
            }
            catch (Exception ex)
            {
                this.AddMessageToChat($"Ошибка: {ex.Message}");
            }
        }

        #region Handlers

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessageAsync();
                e.Handled = true; // Предотвращает добавление новой строки в текстовое поле
            }
        }

        private async Task SendMessageAsync()
        {
            string message = MessageBox.Text;
            if (string.IsNullOrEmpty(message)) return;
            if (stream == null)
            {
                this.AddMessageToChat("Нет подключения к серверу");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                this.AddMessageToChat($"[{Users.Last().UserName}] {message}");
                MessageBox.Clear();
            }
            catch (Exception ex)
            {
                this.AddMessageToChat($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        private void DisplayServerName(object sender, RoutedEventArgs e)
        {
            ServerName.Text = $"Сервер: {this.serverIdentifier}";
        }

        private void AddMessageToChat(string message)
        {
            LoggingHelper.AddMessageToTextBox(this.ChatBox, message);
        }

        #endregion
    }
}