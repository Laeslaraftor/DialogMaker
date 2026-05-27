namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptNumberToken(double value) : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.Number;
        public double Value { get; } = value;

        #region Управление

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }

        #endregion
    }
}
