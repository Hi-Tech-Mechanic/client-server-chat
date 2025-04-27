using System;
using System.Net.Sockets;
using System.Text;

namespace ClientApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string serverIp = "127.0.0.1";
            int port = 5001;
            TcpClient client = new TcpClient();

            try
            {
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Connected to server");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }

            NetworkStream stream = client.GetStream();
            string message = "Hello, Server!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string responce = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {responce}");

            client.Close();
        }
    }
}
