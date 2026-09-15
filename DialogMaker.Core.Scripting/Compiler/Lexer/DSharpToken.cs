namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    /// <summary>
    /// Token of D#
    /// </summary>
    public class DSharpToken(DSharpTokenType type, string value, int line, int column, string? filePath = null)
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
        public string? FilePath { get; } = filePath;

        #region Управление

        /// <summary>
        /// Get position string for current token
        /// </summary>
        /// <returns>Position string</returns>
        public string ToPositionString()
        {
            var result = $"at line {Line}:{Column}";

            if (FilePath != null)
            {
                result = $"in \"{FilePath}\" " + result;
            }

            return result;
        }

        public override string ToString()
        {
            return $"{Type}({Value}) {ToPositionString()}";
        }

        #endregion
    }
}
