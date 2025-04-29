namespace Helpers
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty(this string? text)
        {
            if (text == null || text == "")
            {
                return true;
            }

            return false;
        }
    }
}
