using System.Text;

namespace DevToolVault.Utils
{
    public static class StringExtensions
    {
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 0)
                return string.Empty;

            var builder = new StringBuilder(input.Length * count);
            for (int i = 0; i < count; i++)
            {
                builder.Append(input);
            }
            return builder.ToString();
        }
    }
}