namespace MemoDown.Extensions
{
    public static class StringExtensions
    {
        public static string TrimStart(this string str, string value, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (str.StartsWith(value, stringComparison))
            {
                return str.Substring(value.Length);
            }
            return str;
        }

        public static string TrimEnd(this string str, string value, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (str.EndsWith(value, stringComparison))
            {
                var idx = str.LastIndexOf(value, stringComparison);
                return str.Substring(0, idx);
            }

            return str;
        }
    }
}
