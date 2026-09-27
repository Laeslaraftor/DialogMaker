using DialogMaker.Core.Scripting.Compiler;
using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Diagnostics;

namespace DialogMaker.Core.Tests
{
    public static class AstTests
    {
        [Test]
        public static void TestCapturing()
        {
            var script = DSharpAstParser.ParseScript(ScriptLexerTests.CapturingTestPath);
            var firstStatement = script.Statements.FirstOrDefault();

            if (firstStatement == null)
            {
                Debug.Fail("Script has no statements");
                return;
            }
            else if (firstStatement is not InvokableStatementNode invokable)
            {
                Debug.Fail($"First statement should be {nameof(InvokableStatementNode)}");
                return;
            }

            DSharpVariablesVisitor variablesVisitor = new();
            firstStatement.Accept(variablesVisitor, DSharpAstVisitMode.Recursive);

            static void PrintScopeVariables(DSharpScopeVisitor.Scope scope, int indent)
            {
                var tab = GetIndent(indent);
                Console.WriteLine($"{tab}Scope at {GetPosition(scope.Node)} ({string.Join(", ", scope.Variables)})");

                if (scope.InnerScopes != null)
                {
                    foreach (var inner in scope.InnerScopes.Values)
                    {
                        PrintScopeVariables(inner, indent + 1);
                    }
                }
            }

            if (variablesVisitor.RootScope == null)
            {
                Debug.Fail("Root scope is null");
                return;
            }

            PrintScopeVariables(variablesVisitor.RootScope, 0);

            variablesVisitor.Reset(false);
            firstStatement.Accept(variablesVisitor, DSharpAstVisitMode.Recursive);

            string indent1 = GetIndent(1);

            Console.WriteLine();

            foreach (var info in variablesVisitor.RootCaptures)
            {
                foreach (var capture in info.Value.Values)
                {
                    Console.WriteLine($"Capture at {GetPosition(info.Key.Node)} for \"{capture.VariableName}\":");

                    foreach (var scopeInfo in capture.Scopes)
                    {
                        Console.WriteLine($"{indent1}Scope at {GetPosition(scopeInfo.Key.Node)}: {string.Join(", ", scopeInfo.Value.Select(GetPosition))}");
                    }
                }
            }
        }
        [Test]
        public static void TestGetCapturingInfo()
        {
            var script = DSharpAstParser.ParseScript(ScriptLexerTests.CapturingTestPath);
            var firstStatement = script.Statements.FirstOrDefault();

            if (firstStatement == null)
            {
                Debug.Fail("Script has no statements");
                return;
            }
            if (!DSharpCaptureInfo.TryFind(firstStatement, out var info))
            {
                Console.WriteLine("Capture info not found");
                return;
            }

            foreach (var capture in info.Captures)
            {
                Console.WriteLine(capture);
            }
        }

        private static string GetPosition(AstNode node)
        {
            return $"{node.Line}:{node.Column}";
        }
        private static string GetIndent(int indent)
        {
            if (0 >= indent)
            {
                return string.Empty;
            }

            string result = string.Empty;

            for (int i = 0; i < indent; i++)
            {
                result += "    ";
            }

            return result;
        }
    }
}
