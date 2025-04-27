namespace ChatClient
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Net.Sockets;
    using Server;
    using System.ComponentModel;
    using Data;
    using Helpers;

    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private string selectedServerIdentifier = string.Empty;

        public LoginForm()
        {
            InitializeComponent();

            this.Loaded += DisplayServersHandler;

            //if (ServerModel.IsAlive)
            //{
            //    ServerModel.ServersCountChanged += DisplayServersHandler;
            //}
            //else
            //{
            //}
            this.Closing += Dispose;
        }

        #region ViewHandlers

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var userName = LoginTextBox.Text;
            var selectedServerIndex = this.GetSelectedServerIndex(selectedServerIdentifier);

            var user = new User(new TcpClient(), userName, ServerModel.GetPort(selectedServerIndex));
            List<User> users = [user];

            ChatForm chatForm = new ChatForm(users, selectedServerIdentifier);
            chatForm.Show();

            this.Close();
        }

        private void SelectServer_Click(object sender, SelectionChangedEventArgs e)
        {
            if (this.ServersComboBox.SelectedItem == null)
                return;

            this.SelectServer(e.AddedItems[0]?.ToString());
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerWindow server = new ServerWindow();
                server.Show();

                var lastServerIndex = ServerModel.TcpListeners.Count - 1;
                var serverAdress = ServerModel.GetLocalEndPoint(lastServerIndex)?.ToString();
                this.AddServerToComboBox(serverAdress!);
                this.SelectServer(serverAdress);
                //this.ServersComboBox.SelectedIndex = lastServerIndex;

                this.ContinueButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                throw new NullReferenceException($"Не удалось получить список серверов: {ex}");
            }
        }

        #endregion

        private void Dispose(object? sender, CancelEventArgs e)
        {
            ServerModel.ServersCountChanged -= DisplayServersHandler;
            this.Loaded -= DisplayServersHandler;
            this.Closing -= Dispose;
        }

        private void DisplayServersHandler()
        {
            DisplayServers();
        }

        private void DisplayServersHandler(object sender, RoutedEventArgs e)
        {
            DisplayServers();
        }

        private void DisplayServers()
        {
            var localListeners = ServerModel.TcpListeners;
            if (localListeners == null)
                return;

            foreach (var listener in localListeners)
            {
                if (listener == null)
                    continue;

                var serverAdress = $"{listener.LocalEndpoint}";
                this.AddServerToComboBox(serverAdress);
                this.SelectServer(serverAdress);
            }

            if (ServersAreMissing() == false)
            {
                this.ContinueButton.IsEnabled = true;
            }
        }

        private void AddServerToComboBox(string serverAdress)
        {
            this.ServersComboBox.Items.Add(serverAdress);
        }

        private void SelectServer(string? serverAdress)
        {
            if (serverAdress.IsNullOrEmpty())
            {
                this.ServersComboBox.SelectedIndex = 0;
            }
            else
            {
                this.ServersComboBox.SelectedIndex = GetSelectedServerIndex(serverAdress!);
            }

            this.selectedServerIdentifier = this.ServersComboBox.SelectedItem.ToString()!;
        }

        private int GetSelectedServerIndex(string serverAdress)
        {
            var result = this.ServersComboBox.Items.Cast<string>().ToList().FindIndex(item => item == serverAdress);
            return result;
        }

        private bool ServersAreMissing()
        {
            var isMissing = this.ServersComboBox.Items.Count == 0;
            return isMissing;
        }
    }
}
