using System;

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
            string currentValue = string.Empty;

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
                        handler(currentValue);
                        currentValue = string.Empty;
                        continue;
                    }
                }

                currentValue += c;
                skipNext = false;
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                handler(currentValue);
            }
        }
    }
}
