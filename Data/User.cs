namespace Data
{
    using System.Net.Sockets;

    public class User
    {
        public TcpClient TcpClient = null!;

        public int UsedServerPort;

        public string UserName
        {
            get => userName;
            set => userName = string.IsNullOrEmpty(value) ? $"User_{defaultUsersCount}" : value;
        }
        private string userName = "Default User";

        private static int defaultUsersCount;

        public User(TcpClient TcpClient, string UserName, int UsedServerPort)
        {
            this.TcpClient = TcpClient;
            this.UserName = UserName;
            this.UsedServerPort = UsedServerPort;
            defaultUsersCount++;
        }
    }
}
