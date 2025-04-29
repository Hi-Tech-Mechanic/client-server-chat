namespace Helpers
{
    using System.Windows.Controls;

    public static class LoggingHelper
    {
        public static void WriteLineToConsole(string? message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public static void AddMessageToTextBox(TextBox textBox, string? message)
        {
            if (message.IsNullOrEmpty())
                return;

            textBox.AppendText(WriteLine(message!));
        }

        private static string WriteLine(string message)
        {
            return message + "\n";
        }
    }
}
