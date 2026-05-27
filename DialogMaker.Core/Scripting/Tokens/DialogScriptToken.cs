namespace DialogMaker.Core.Scripting.Tokens
{
    public abstract class DialogScriptToken
    {
        public abstract DialogScriptTokenType Type { get; }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
