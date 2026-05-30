namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    /// <summary>
    /// Token of dialog script
    /// </summary>
    public class DialogScriptToken(DialogScriptTokenType type, string value, int line, int column)
        : IEquatable<DialogScriptToken>
    {

        /// <summary>
        /// Type of this token
        /// </summary>
        public DialogScriptTokenType Type { get; } = type;
        /// <summary>
        /// Token value
        /// </summary>
        public string Value { get; } = value;
        /// <summary>
        /// Token line index
        /// </summary>
        public int Line { get; } = line;
        /// <summary>
        /// Token column start index
        /// </summary>
        public int Column { get; } = column;

        #region Операторы

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(DialogScriptToken? l, DialogScriptToken? r) => Equals(l, r);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="l"><inheritdoc/></param>
        /// <param name="r"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(DialogScriptToken? l, DialogScriptToken? r) => !Equals(l, r);

        #endregion

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value, Line, Column);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override bool Equals(object obj)
        {
            return obj is DialogScriptToken other &&
                   Equals(other);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Equals(DialogScriptToken? other)
        {
            if (other == null)
            {
                return false;
            }

            return Type == other.Type &&
                   Value == other.Value &&
                   Line == other.Line &&
                   Column == other.Column;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"{Type}({Value}) at {Line}:{Column}";
        }

        #endregion
    }
}
