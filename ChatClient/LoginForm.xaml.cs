namespace ChatClient
{
    using Data;
    using Helpers;
    using Server;
    using System.ComponentModel;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Логика взаимодействия для LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private string selectedServerIdentifier = string.Empty;

        public LoginForm()
        {
            InitializeComponent();

            ServerModel.ServersCountChanged += DisplayServersHandler; // При загрузке не срабатывает, зато в течении работы да
            this.Loaded += DisplayServersHandler; // Для инициализации формы
            this.Closing += Dispose;
        }

        #region ViewHandlers

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var userName = LoginTextBox.Text;
            var selectedServerIndex = this.GetSelectedServerIndex(selectedServerIdentifier);

            var user = new User(new TcpClient(), userName, ServerModel.GetPort(selectedServerIndex));

            ChatForm chatForm = new ChatForm(user, selectedServerIdentifier);
            chatForm.Show();

            this.Close();
        }

        private void SelectServer_Click(object sender, SelectionChangedEventArgs e)
        {
            if (this.ServersComboBox.SelectedItem == null)
                return;

            var serverAdress = e.AddedItems[0]?.ToString();
            if (serverAdress.IsNullOrEmpty() == false)
            {
                this.selectedServerIdentifier = serverAdress!;
            }
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerWindow server = new ServerWindow();
                server.Show();

                var lastServerIndex = ServerModel.TcpListeners.Count - 1;
                var serverAdress = ServerModel.GetLocalEndPoint(lastServerIndex)?.ToString();
                this.SelectServer(serverAdress);

                this.ContinueButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                throw new NullReferenceException($"Не удалось получить список серверов: {ex}");
            }
        }

        private void DeleteAllServers_Click(object sender, RoutedEventArgs e)
        {
            ServerModel.ClearAllServers();
            this.RemoveAllServersFromComboBox();
        }

        #endregion

        #region ServersMethods

        private void DisplayServersHandler(object sender, RoutedEventArgs e)
        {
            this.DisplayServers();
            this.UpdateButtonsState();
        }

        private void DisplayServersHandler()
        {
            this.DisplayServers();
            this.UpdateButtonsState();
        }

        private void DisplayServers()
        {
            var localListeners = ServerModel.TcpListeners;
            if (localListeners == null)
            return;

            string lastServerAdress = string.Empty;
            this.RemoveAllServersFromComboBox();

            foreach (var listener in localListeners)
            {
                if (listener == null)
                    continue;

                var serverAdress = $"{listener.LocalEndpoint}";
                lastServerAdress = serverAdress;
                this.AddServerToComboBox(serverAdress);
            }

            this.SelectServer(lastServerAdress);
        }

        private void UpdateButtonsState()
        {
            if (ServerModel.TcpListeners == null)
            {
                this.ContinueButton.IsEnabled = false;
                this.DeleteAllServersButton.IsEnabled = false;

                return;
            }
            else
            {
                if (this.ServersComboBox.SelectedItem != null)
                {
                    this.ContinueButton.IsEnabled = true;
                }
                this.DeleteAllServersButton.IsEnabled = true;
            }
        }

        private void SelectServer(string? serverAdress)
        {
            if (serverAdress.IsNullOrEmpty())
            {
                this.ServersComboBox.SelectedIndex = 0;
                return;
            }
            else
            {
                this.ServersComboBox.SelectedIndex = GetSelectedServerIndex(serverAdress!);
                this.selectedServerIdentifier = serverAdress!;
            }
        }

        private void AddServerToComboBox(string serverAdress)
        {
            this.ServersComboBox.Items.Add(serverAdress);
        }

        private void RemoveAllServersFromComboBox()
        {
            this.ServersComboBox.Items.Clear();
        }

        private int GetSelectedServerIndex(string serverAdress)
        {
            var result = this.ServersComboBox.Items
                .Cast<string>()
                .ToList()
                .FindIndex(item => item == serverAdress);

            return result;
        }

        private static bool IsLocalPortActive(int port)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var listeners = properties.GetActiveTcpListeners();
            var portIsActive = listeners.Any(x => x.Port == port);

            return portIsActive;
        }

        #endregion

        private void Dispose(object? sender, CancelEventArgs e)
        {
            ServerModel.ServersCountChanged -= DisplayServersHandler;
            this.Closing -= Dispose;
        }
    }
}
