namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptVariableToken(bool isDefinition, string name, DialogScriptToken? value) : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.Variable;
        public bool IsDefinition { get; } = isDefinition;
        public string Name { get; } = name;
        public DialogScriptToken Value { get; } = value ?? new DialogScriptNullToken();

        #region Управление

        public override string ToString()
        {
            if (IsDefinition)
            {
                return $"{Type}: var {Name} = {Value}";
            }

            return $"{Type}: {Name} = {Value}";
        }

        #endregion
    }
}
