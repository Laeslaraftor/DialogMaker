using DialogMaker.Core.Scripting.Data;

namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptAttributeTokenParser : DialogScriptTokenParser
    {
        public override bool CanParse(string token)
        {
            return token[0] == AttributeStart && token[^1] == AttributeEnd;
        }
        public override DialogScriptToken Parse(StringStream value)
        {
            value.ReadWhile(c => c != AttributeEnd && c != ' ', true, out var content);

            if (string.IsNullOrEmpty(content) ||
                content[0] != AttributeStart ||
                content[^1] != AttributeEnd)
            {
                return new DialogScriptUnknownToken(content);
            }

            string attributeName = content[1..^1];
            return new DialogScriptAttributeToken(attributeName);
        }

        #region Константы

        public const char AttributeStart = '[';
        public const char AttributeEnd = ']';

        #endregion

        #region Статика

        public static readonly DialogScriptAttributeTokenParser Instance = new();

        #endregion
    }
}
