using DialogMaker.Core.Scripting.Data;

namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptStringTokenParser : DialogScriptTokenParser
    {
        public override bool CanParse(string token)
        {
            return token[0] == '"' && token[^1] == '"';
        }
        public override DialogScriptToken Parse(StringStream value)
        {
            var content = ReadString(value);
            return new DialogScriptStringToken(content[1..^1]);
        }

        #region Константы

        public const char DoubleQuotes = '"';
        public const char Quotes = '\'';

        #endregion

        #region Статика

        public static readonly DialogScriptStringTokenParser Instance = new();

        public static string ReadString(StringStream stream)
        {
            bool stringStarted = false;
            bool ignoreNext = false;
            char quotes = Quotes;
            bool removeLastSymbol = false;
            var result = stream.ReadWhile(value =>
            {
                if (ignoreNext)
                {
                    ignoreNext = false;
                    return false;
                }
                if (!stringStarted)
                {
                    if (IsSeparatorAllowSpace(value))
                    {
                        removeLastSymbol = true;
                        return true;
                    }
                    if (IsQuotes(value))
                    {
                        quotes = value;
                        stringStarted = true;
                    }

                    return false;
                }
                if (value == '\\')
                {
                    ignoreNext = true;
                    return false;
                }

                return value == quotes;
            }).Trim();

            if (removeLastSymbol)
            {
                result = result[0..^1];
            }

            return result;
        }
        public static bool IsQuotes(char value)
        {
            return value == DoubleQuotes || value == Quotes;
        }

        #endregion
    }
}
