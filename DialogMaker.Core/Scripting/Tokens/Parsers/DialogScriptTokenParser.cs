using DialogMaker.Core.Scripting.Data;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Tokens
{
    public abstract class DialogScriptTokenParser
    {
        public abstract bool CanParse(string token);
        public abstract DialogScriptToken Parse(StringStream value);

        #region Константы

        public const char Semicolon = ';';
        public const char Space = ' ';
        public const char Comma = ',';

        #endregion

        #region Статика

        public static ReadOnlyDictionary<DialogScriptTokenType, DialogScriptTokenParser> AvailableParsers
        {
            get
            {
                if (field == null)
                {
                    Dictionary<DialogScriptTokenType, DialogScriptTokenParser> parsers = [];

                    foreach (var tokenType in Enum.GetValues(typeof(DialogScriptTokenType)))
                    {
                        var parserAttribute = tokenType.GetEnumAttribute<ParserAttribute>();

                        if (parserAttribute == null)
                        {
                            continue;
                        }

                        var instanceField = parserAttribute.Type.GetField("Instance");
                        var instance = instanceField?.GetValue(null) as DialogScriptTokenParser;
                        instance ??= (DialogScriptTokenParser)Activator.CreateInstance(parserAttribute.Type);

                        parsers.Add((DialogScriptTokenType)tokenType, instance);
                    }

                    field = new(parsers);
                }

                return field;
            }
        }

        public static bool TryGetParser(string token, [NotNullWhen(true)] out DialogScriptTokenParser? result)
        {
            result = null;

            foreach (var parser in AvailableParsers.Values)
            {
                if (parser.CanParse(token))
                {
                    result = parser;
                    return true;
                }
            }

            return false;
        }

        public static bool IsNotSeparator(char value) => !IsSeparator(value);
        public static bool IsSeparator(char value)
        {
            return value == Semicolon || value == Comma || value == Space;
        }
        public static bool IsSeparatorAllowSpace(char value)
        {
            return value == Semicolon || value == Comma;
        }

        #endregion
    }
}
