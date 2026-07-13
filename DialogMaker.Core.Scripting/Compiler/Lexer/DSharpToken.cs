namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    /// <summary>
    /// Token of D#
    /// </summary>
    public class DSharpToken(DSharpTokenType type, string value, int line, int column)
    {
        /// <summary>
        /// Type of this token
        /// </summary>
        public DSharpTokenType Type { get; } = type;
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

        #region Управление

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
