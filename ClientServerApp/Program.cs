using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }
        
    private static async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Received: {message}");

        string response = "Message received";
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

        client.Close();
    }

    #region Functionality check

    private static void DisplayTCPConnections()
    {
        var ipProps = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnections = ipProps.GetActiveTcpConnections();

        Console.WriteLine($"Всего {tcpConnections.Length} активных TCP-подключений");
        Console.WriteLine();
        foreach (var connection in tcpConnections)
        {
            Console.WriteLine("=============================================");
            Console.WriteLine($"Локальный адрес: {connection.LocalEndPoint.Address}:{connection.LocalEndPoint.Port}");
            Console.WriteLine($"Адрес удаленного хоста: {connection.RemoteEndPoint.Address}:{connection.RemoteEndPoint.Port}");
            Console.WriteLine($"Состояние подключения: {connection.State}");
        }
    }

    private static void DisplayTrafic()
    {
        var ipProps = IPGlobalProperties.GetIPGlobalProperties();
        var ipStats = ipProps.GetIPv4GlobalStatistics();
        Console.WriteLine($"Входящие пакеты: {ipStats.ReceivedPackets.ConvertToMegabites()}");
        Console.WriteLine($"Исходящие пакеты: {ipStats.OutputPacketRequests.ConvertToMegabites()}");
        Console.WriteLine($"Отброшено входящих пакетов: {ipStats.ReceivedPacketsDiscarded}");
        Console.WriteLine($"Отброшено исходящих пакетов: {ipStats.OutputPacketsDiscarded}");
        Console.WriteLine($"Ошибки фрагментации: {ipStats.PacketFragmentFailures}");
        Console.WriteLine($"Ошибки восстановления пакетов: {ipStats.PacketReassemblyFailures}");
    }

    private static void DisplayAllNetworkInterfaces()
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        Console.WriteLine($"Обнаружено {adapters.Length} устройств");
        foreach (NetworkInterface adapter in adapters)
        {
            Console.WriteLine("=====================================================================");
            Console.WriteLine();
            Console.WriteLine($"ID устройства: ------------- {adapter.Id}");
            Console.WriteLine($"Имя устройства: ------------ {adapter.Name}");
            Console.WriteLine($"Описание: ------------------ {adapter.Description}");
            Console.WriteLine($"Тип интерфейса: ------------ {adapter.NetworkInterfaceType}");
            Console.WriteLine($"Физический адрес: ---------- {adapter.GetPhysicalAddress()}");
            Console.WriteLine($"Статус: -------------------- {adapter.OperationalStatus}");
            Console.WriteLine($"Скорость: ------------------ {adapter.Speed}");

            IPInterfaceStatistics stats = adapter.GetIPStatistics();
            Console.WriteLine($"Получено: ----------------- {stats.BytesReceived}");
            Console.WriteLine($"Отправлено: --------------- {stats.BytesSent}");
        }
    }

    private static string ConvertToMegabites(this long bytes) 
    {
        var megabites = (float)Math.Round((bytes / 1024f / 1024f), 2);
        return $"{megabites} Mb";
    }

    #endregion
}