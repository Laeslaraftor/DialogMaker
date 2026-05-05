namespace DialogMaker.Core.Executioning
{
    internal static class ParseHelper
    {
        public static void Parse(string? value, char separator, Action<string> handler)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            bool skipNext = false;
            string? currentValue = null;

            foreach (var c in value)
            {
                if (!skipNext)
                {
                    if (c == '\\')
                    {
                        skipNext = true;
                        continue;
                    }
                    else if (c == separator)
                    {
                        handler(currentValue ?? string.Empty);
                        currentValue = string.Empty;
                        continue;
                    }
                }

                currentValue += c;
                skipNext = false;
            }

            if (currentValue != null)
            {
                handler(currentValue);
            }
        }
    }
}
