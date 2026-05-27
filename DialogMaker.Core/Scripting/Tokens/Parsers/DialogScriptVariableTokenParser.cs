using DialogMaker.Core.Scripting.Data;

namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptVariableTokenParser : DialogScriptTokenParser
    {
        public override bool CanParse(string token)
        {
            return token.StartsWith(VarKeyword);
        }
        public override DialogScriptToken Parse(StringStream value)
        {
            bool nowKeyword = true;
            bool previousIsSpace = false;
            bool assignmentExists = false;
            string varKeyword = string.Empty;
            string variableName = string.Empty;
            var definitionContent = value.ReadWhile(c =>
            {
                if (IsSeparatorAllowSpace(c))
                {
                    return true;
                }
                if (c == Space)
                {
                    previousIsSpace = true;
                    return false;
                }
                else if (c == AssignmentOperator)
                {
                    assignmentExists = true;
                    previousIsSpace = false;
                    return true;
                }
                if (previousIsSpace)
                {
                    variableName += ' ';
                }

                if (nowKeyword)
                {
                    if (c == Space)
                    {
                        nowKeyword = false;
                    }
                    else
                    {
                        varKeyword += c;
                    }
                }
                else
                {
                    variableName += c;
                }


                return false;
            });
            var assignmentContent = string.Empty;
            DialogScriptToken? variableValue = null;

            if (!string.IsNullOrEmpty(varKeyword) && 
                varKeyword != VarKeyword &&
                string.IsNullOrEmpty(variableName))
            {
                variableName = varKeyword;
                varKeyword = string.Empty;
            }

            if (assignmentExists)
            {
                assignmentContent = DialogScriptStringTokenParser.ReadString(value);

                if (TryGetParser(assignmentContent, out var parser))
                {
                    variableValue = parser.Parse(new(assignmentContent));
                }
            }

            if (variableName.Contains(Space) ||
                (!string.IsNullOrEmpty(varKeyword) && varKeyword != VarKeyword))
            {
                return new DialogScriptUnknownToken(definitionContent + assignmentContent);
            }

            return new DialogScriptVariableToken(!string.IsNullOrEmpty(varKeyword), variableName, variableValue);
        }

        #region Константы

        public const string VarKeyword = "var";
        public const char AssignmentOperator = '=';

        #endregion

        #region Статика

        public static readonly DialogScriptVariableTokenParser Instance = new();

        #endregion
    }
}
