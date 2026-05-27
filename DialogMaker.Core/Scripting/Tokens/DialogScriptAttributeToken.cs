namespace DialogMaker.Core.Scripting.Tokens
{
    public class DialogScriptAttributeToken(string name) : DialogScriptToken
    {
        public override DialogScriptTokenType Type => DialogScriptTokenType.Attribute;
        public string Name { get; } = name;

        #region Управление

        public bool TryParseNameToEnum<T>(out T result)
            where T : struct
        {
            return Enum.TryParse(Name, out result);
        }

        public override string ToString()
        {
            return $"{Type}: [{Name}]";
        }

        #endregion
    }
}
