namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    internal static class LexerExtensions
    {
        private static readonly char[] _hexLetters = ['A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f'];

        extension(char)
        {

            public static bool IsHexLetter(char value)
            {
                return _hexLetters.Contains(value);
            }
        }
    }
}
