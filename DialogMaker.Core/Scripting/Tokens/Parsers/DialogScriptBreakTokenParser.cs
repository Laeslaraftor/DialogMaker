namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptBreakTokenParser : DialogScriptKeywordTokenParser
    {
        protected override string Keyword => BreakKeyword;

        protected override DialogScriptToken CreateToken(string content)
        {
            return new DialogScriptBreakToken();
        }

        #region Константы

        public const string BreakKeyword = "break";

        #endregion

        #region Статика

        public static DialogScriptBreakTokenParser Instance = new();

        #endregion
    }
}
