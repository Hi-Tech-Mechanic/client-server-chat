using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public static class Program
    {
        internal static List<TcpListener>? currentServer;
        private static readonly List<TcpClient> clients = new List<TcpClient>();

        static async Task Main(string[] args)
        {
            int port = 5000;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            currentServer?.Add(server);
            server.Start();
            Console.WriteLine($"Server started on port {port}");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                Console.WriteLine("Client connected: " + client.Client.RemoteEndPoint);
                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int byteRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, byteRead);
                    Console.WriteLine("Received from " + client.Client.RemoteEndPoint + ": " + message);

                    await BroadcastMessageAsync(message, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling client " + client.Client.RemoteEndPoint + ": " + ex.Message);
            }
            finally
            {
                clients.Remove(client);
                Console.WriteLine("Client disconnected: " + client.Client.RemoteEndPoint);
                client.Close();
            }
        }

        private static async Task BroadcastMessageAsync(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);  

            foreach (TcpClient client in clients)
            {
                if (client != sender)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        await stream.WriteAsync(data, 0, data.Length);
                        Console.WriteLine("Отправил к" + client.Client.RemoteEndPoint + ": " + message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка отправки к " + client.Client.RemoteEndPoint + ": " + ex.Message);
                    }
                }
            }
        }

        //static async Task HandleClientAsync(TcpClient client)
        //{
        //    NetworkStream stream = client.GetStream();
        //    byte[] buffer = new byte[1024];
        //    int bytesRead;

        //    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //    {
        //        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //        Console.WriteLine($"Received: {message}");

        //        // Отправка сообщения всем клиентам (в данном примере только одному)
        //        await stream.WriteAsync(buffer, 0, bytesRead);
        //    }

        //    client.Close();
        //}
    }
}