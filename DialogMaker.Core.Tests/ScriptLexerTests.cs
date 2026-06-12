using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Tests
{
    internal class ScriptLexerTests
    {
        public const string ScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\DialogScriptExample.txt";
        public const string SimpleScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\SimpleScriptExample.txt";
        public const string ExpressionsScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\ExpressionsScript.txt";
        public const string MathScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\MathScript.txt";
        public const string TypeDetectionScriptPath = @"F:\Projects\DialogMaker\DialogMaker.Core.Tests\CodeExample\TypeDetectionScript.txt";

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
            Console.WriteLine();
            Console.WriteLine("Functions:");

            foreach (var statement in program.Statements)
            {
                if (statement is InvokableStatementNode invokableStatement)
                {
                    Console.WriteLine(invokableStatement.ToString().Trim());
                    Console.WriteLine();
                }
            }
        }

        private static DSharpLexer GetLexer()
        {
            var script = File.ReadAllText(MathScriptPath);
            return new(script);
        }
    }
}
