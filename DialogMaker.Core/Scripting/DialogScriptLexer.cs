using DialogMaker.Core.Scripting.Data;
using DialogMaker.Core.Scripting.Tokens;

namespace DialogMaker.Core.Scripting
{
    public class DialogScriptLexer
    {
        public static DialogScript Tokenize(string script)
        {
            List<DialogScriptToken> tokens = [];
            StringStream stream = new(script);
            string currentToken = string.Empty;
            int currentTokenStringPosition = 0;
            char currentValue;

            do
            {
                currentValue = stream.ReadChar();

                if (string.IsNullOrEmpty(currentToken) && currentValue == ' ')
                {
                    currentTokenStringPosition++;
                    continue;
                }
                else if (DialogScriptStringTokenParser.IsQuotes(currentValue))
                {
                    stream.Position--;
                    currentToken += DialogScriptStringTokenParser.ReadString(stream);
                }
                else
                {
                    currentToken += currentValue;
                }

                if (DialogScriptTokenParser.TryGetParser(currentToken, out var parser))
                {
                    stream.Position = currentTokenStringPosition;
                    var token = parser.Parse(stream);
                    tokens.Add(token);
                    currentToken = string.Empty;
                    currentTokenStringPosition = stream.Position;
                }
                else if (DialogScriptTokenParser.IsSeparator(currentValue) && !string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(new DialogScriptUnknownToken(currentToken));
                    currentToken = string.Empty;
                    currentTokenStringPosition = stream.Position;
                }
            }
            while (currentValue != '\0');

            return new(tokens);
        }


    }
}
