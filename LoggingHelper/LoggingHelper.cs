
using System.Windows.Controls;

namespace Helpers
{
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

        public static bool IsNullOrEmpty(this string? text)
        {
            if (text == null || text == "")
            {
                return true;
            }

            return false;
        }

        private static string WriteLine(string message)
        {
            return message + "\n";
        }
    }
}
