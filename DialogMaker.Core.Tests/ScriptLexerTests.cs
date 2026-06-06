using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Tests
{
    internal class ScriptLexerTests
    {
        public const string ScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\DialogScriptExample.txt";
        public const string SimpleScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\SimpleScriptExample.txt";

        [Test]
        public static void Tokenize()
        {
            var lexer = GetLexer();
            lexer.Tokenize();

            foreach (var token in lexer.Tokens)
            {
                Console.WriteLine(token);
            }
        }
        [Test]
        public static void ParseProgram()
        {
            var lexer = GetLexer();
            lexer.Tokenize();
            AstParser parser = new(lexer);
            var program = parser.Parse("Example");

            Console.WriteLine(program.ToString());
        }

        public static DSharpLexer GetLexer()
        {
            var script = File.ReadAllText(ScriptPath);
            return new(script);
        }
    }
}
