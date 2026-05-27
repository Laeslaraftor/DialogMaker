namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptUnknownToken(string content) : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.Unknown;
        public string Content { get; } = content;
    }
}
