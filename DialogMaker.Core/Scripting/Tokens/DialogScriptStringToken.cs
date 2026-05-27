namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptStringToken(string value) : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.String;
        public string Value { get; } = value;

        #region Управление

        public override string ToString()
        {
            return $"{Type}: \"{Value}\"";
        }

        #endregion
    }
}
