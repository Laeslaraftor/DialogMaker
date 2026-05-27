namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptNullTokenParser : DialogScriptKeywordTokenParser
    {
        protected override string Keyword => Null;

        protected override DialogScriptToken CreateToken(string content)
        {
            return new DialogScriptNullToken();
        }

        #region Константы

        public const string Null = "null";

        #endregion

        #region Статика

        public static readonly DialogScriptNullTokenParser Instance = new();

        #endregion
    }
}
