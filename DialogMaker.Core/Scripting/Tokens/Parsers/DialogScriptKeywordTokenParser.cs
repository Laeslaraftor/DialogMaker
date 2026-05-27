using DialogMaker.Core.Scripting.Data;

namespace DialogMaker.Core.Scripting.Tokens
{
    public abstract class DialogScriptKeywordTokenParser : DialogScriptTokenParser
    {
        protected abstract string Keyword { get; }

        public override bool CanParse(string token) => token == Keyword;
        public override DialogScriptToken Parse(StringStream value)
        {
            var content = value.ReadWhile(IsNotSeparator).Trim();

            if (content != Keyword)
            {
                return new DialogScriptUnknownToken(content);
            }

            return CreateToken(content);
        }

        protected abstract DialogScriptToken CreateToken(string content);
    }
}
