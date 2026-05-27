namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptContinueTokenParser : DialogScriptKeywordTokenParser
    {
        protected override string Keyword => ContinueKeyword;

        protected override DialogScriptToken CreateToken(string content)
        {
            return new DialogScriptContinueToken();
        }

        #region Константы

        public const string ContinueKeyword = "continue";

        #endregion

        #region Статика

        public static DialogScriptContinueTokenParser Instance = new();

        #endregion
    }
}
