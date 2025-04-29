namespace ChatClient
{
    using Data;
    using Helpers;
    using System.Net.Sockets;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ChatForm : Window
    {
        /// <summary>
        /// Измерение в байтах
        /// </summary>
        private const ushort bufferSize = 1024;

        private readonly User User; // warn возмонж не надо использовать список, так как в одном экземпляре используется 1 пользователь
        private NetworkStream stream = null!;

        private string ServerIdentifier { get; }

        public ChatForm(User user, string serverIdentifier)
        {
            InitializeComponent();

            this.User = user;
            this.ServerIdentifier = serverIdentifier;
            this.Loaded += DisplayServerName;

            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                var user = this.User;
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
                    int bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Dispatcher.Invoke(() =>
                    {
                        this.AddMessageToChat(message);
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
            if (this.stream == null)
            {
                this.AddMessageToChat("Нет подключения к серверу");
                return;
            }

            string enteredMessage = MessageBox.Text;
            if (string.IsNullOrEmpty(enteredMessage))
                return;

            string userName = GetUserNameDecoration(User.UserName);
            string fullMessage = $"{userName} {MessageBox.Text}";

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(fullMessage);
                await stream.WriteAsync(data);
                this.AddMessageToChat(fullMessage);
                MessageBox.Clear();
            }
            catch (Exception ex)
            {
                this.AddMessageToChat($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        private void DisplayServerName(object sender, RoutedEventArgs e)
        {
            ServerName.Text = $"Сервер: {this.ServerIdentifier}";
        }

        private void AddMessageToChat(string message)
        {
            LoggingHelper.AddMessageToTextBox(this.ChatBox, message);
        }

        #endregion

        private static string GetUserNameDecoration(string userName)
        {
            var result = $"[{userName}]";
            return result;
        }
    }
}