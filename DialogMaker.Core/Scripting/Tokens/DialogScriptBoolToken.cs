namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptBoolToken(bool value) : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.Bool;
        public bool Value { get; } = value;

        #region Управление

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }

        #endregion
    }
}
