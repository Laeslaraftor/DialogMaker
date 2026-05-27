using DialogMaker.Core.Scripting.Data;

namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptBoolTokenParser : DialogScriptTokenParser
    {
        public override bool CanParse(string token)
        {
            return token == True || token == False;
        }
        public override DialogScriptToken Parse(StringStream value)
        {
            var content = value.ReadWhile(IsNotSeparator).Trim();

            if (content == True)
            {
                return new DialogScriptBoolToken(true);
            }
            else if (content == False)
            {
                return new DialogScriptBoolToken(false);
            }

            return new DialogScriptUnknownToken(content);
        }

        #region Константы

        public const string True = "true";
        public const string False = "false";

        #endregion

        #region Статика

        public static readonly DialogScriptBoolTokenParser Instance = new();

        #endregion
    }
}
