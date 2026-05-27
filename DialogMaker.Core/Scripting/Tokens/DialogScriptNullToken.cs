namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptNullToken : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.Null;

        #region Управление

        public override string ToString()
        {
            return $"{Type}: {DialogScriptNullTokenParser.Null}";
        }

        #endregion
    }
}
