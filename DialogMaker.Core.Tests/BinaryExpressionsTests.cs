using DialogMaker.Core.Scripting.Compiler;
using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics;

namespace DialogMaker.Core.Tests
{
    public static class BinaryExpressionsTests
    {
        [TestCase("1 + 1")]
        [TestCase("1 + 1 * 2")]
        [TestCase("(1 + 1) * 2")]
        [TestCase("isOpen || isShowed")]
        [TestCase("a == null && b == null || ReferenceEquals(a, b)")]
        public static void TestBinaryExpressionCompiling(string expression)
        {
            DSharpLexer lexer = new(expression);
            AstParserStream tokensStream = new(lexer);
            lexer.Tokenize();

            var parsedExpression = ExpressionNode.ParseExpression(tokensStream);

            if (parsedExpression is not BinaryExpressionNode binaryExpression)
            {
                Debug.Fail($"Parsed expression is not binary: {parsedExpression}");
                return;
            }

            Console.WriteLine("Binary expression:");
            Console.WriteLine(binaryExpression);
            Console.WriteLine();

            foreach (var pair in DSharpBinaryExpressionCompiler.Compile(binaryExpression))
            {
                Console.WriteLine(pair);
            }
        }
    }
}
