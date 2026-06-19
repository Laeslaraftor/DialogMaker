using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Builders;
using DialogMaker.Core.Scripting.Runtime.Compilers;
using System.Diagnostics;

namespace DialogMaker.Core.Tests
{
    internal class ScriptCompilerTests
    {
        [Test]
        public static void TestSimpleScriptCompiling()
        {
            DSharpAssemblyBuilder assembly = CompileSimpleScript();

            Console.WriteLine("Types:");

            foreach (var type in assembly.Types)
            {
                Console.Write(type.ObjectType.ToString().ToLower() + " ");
                Console.WriteLine(type.FullName);

                if (type.Properties.Count > 0)
                {
                    Console.WriteLine("    Properties:");

                    foreach (var property in type.Properties)
                    {
                        Console.Write("        ");
                        PrintType(property.PropertyType);
                        Console.Write($"{property.Name} {{ ");

                        if (property.Getter != null)
                        {
                            Console.Write("get;");
                        }
                        if (property.Setter != null)
                        {
                            if (property.Getter != null)
                            {
                                Console.Write(" ");
                            }

                            Console.Write("set;");
                        }

                        Console.WriteLine(" }");
                    }
                }
                if (type.Fields.Count > 0)
                {
                    Console.WriteLine("    Fields:");

                    foreach (var field in type.Fields)
                    {
                        Console.Write("        ");
                        PrintField(field);
                    }
                }
                if (type.Methods.Count > 0)
                {
                    Console.WriteLine("    Methods:");

                    foreach (var method in type.Methods)
                    {
                        Console.Write("        ");
                        PrintMethod(method);
                    }
                }
                if (type.Constructors.Count > 0)
                {
                    Console.WriteLine("    Constructors:");

                    foreach (var method in type.Constructors)
                    {
                        Console.Write("        ");
                        PrintMethod(method);
                    }
                }
            }

            void PrintType(DSharpTypeToken? token)
            {
                if (token != null)
                {
                    var type = assembly.GetType(token);

                    if (type is DSharpTypeBuilder builder)
                    {
                        Console.Write($"{builder.FullName} ");
                    }
                    else if (type is DSharpType typeInfo)
                    {
                        Console.Write($"{typeInfo.FullName} ");
                    }
                }
            }
            void PrintRawValue(DSharpFieldBuilder field)
            {
                if (field.RawValue == null)
                {
                    return;
                }

                bool isString = field.RawValue.Value.IsString;

                Console.Write(" = ");

                if (isString)
                {
                    Console.Write('"');
                }

                Console.Write(field.RawValue);

                if (isString)
                {
                    Console.Write('"');
                }
            }
            void PrintField(DSharpFieldBuilder field)
            {
                PrintType(field.FieldType);
                Console.Write(field.Name);
                PrintRawValue(field);
                Console.WriteLine();
            }
            void PrintMethod(DSharpMethodBuilder method)
            {
                PrintType(method.ReturnType);
                Console.Write(method.Name);
                Console.Write('(');

                int parameterIndex = 0;

                foreach (var parameter in method.Parameters)
                {
                    if (parameterIndex > 0)
                    {
                        Console.Write(", ");
                    }

                    PrintType(parameter.Type);
                    Console.Write(parameter.Name);
                    parameterIndex++;
                }

                Console.WriteLine(')');
            }

            Console.WriteLine();
            Console.WriteLine("Global functions:");

            foreach (var function in assembly.GlobalFunctions)
            {
                PrintMethod(function);
            }

            Console.WriteLine();
            Console.WriteLine("Global variables:");

            foreach (var variable in assembly.GlobalVariables)
            {
                PrintField(variable);
            }
        }
        [Test]
        public static void TestMathSimplifying()
        {
            var parsedScript = ParseScript(ScriptLexerTests.MathScriptPath, "SimpleScript");
            VariableNode? variableWithInitializer = null;

            foreach (var statement in parsedScript.Statements)
            {
                if (statement is VariableStatementNode variableStatement &&
                    variableStatement.Variable?.Initializer != null)
                {
                    variableWithInitializer = variableStatement.Variable;
                    break;
                }
            }

            if (variableWithInitializer == null)
            {
                Debug.Fail("Variable with initializer not found");
                return;
            }

            Console.WriteLine("Variable:");
            Console.WriteLine(variableWithInitializer.ToString());

            if (variableWithInitializer.Initializer!.TrySimplifyToLiteral(out var result))
            {
                Console.WriteLine($"Result: {result}");
            }
            else
            {
                Debug.Fail($"Failed to simplify expression: {variableWithInitializer.Initializer}");
            }
        }
        [Test]
        public static void TestExpressionTypeDetection()
        {
            var parsedScript = ParseScript(ScriptLexerTests.TypeDetectionScriptPath, "SimpleScript");
            DSharpAssemblyBuilder assembly = new("SimpleScript", []);
            DSharpCompiler compiler = new(assembly);

            compiler.CompileTrees(parsedScript);

            VariableNode? variableWithInitializer = null;

            foreach (var statement in parsedScript.Statements)
            {
                if (statement is VariableStatementNode variableStatement &&
                    variableStatement.Variable?.Initializer != null)
                {
                    variableWithInitializer = variableStatement.Variable;
                    break;
                }
            }

            if (variableWithInitializer == null)
            {
                Debug.Fail("Variable with initializer not found");
                return;
            }

            Console.WriteLine("Variable:");
            Console.WriteLine(variableWithInitializer.ToString());

            var type = variableWithInitializer.Initializer!.GetExpressionType(assembly);

            Console.WriteLine($"Result: {type}");
        }

        public static DSharpAssemblyBuilder CompileSimpleScript()
        {
            var script = File.ReadAllText(ScriptLexerTests.SimpleScriptPath);
            DSharpLexer lexer = new(script);
            lexer.Tokenize();
            AstParser parser = new(lexer);

            var parsedScript = parser.Parse("SimpleScript");
            DSharpAssemblyBuilder assembly = new("SimpleScript", []);
            DSharpCompiler compiler = new(assembly);

            compiler.CompileTrees(parsedScript);

            return assembly;
        }

        private static DSharpTreeRoot ParseScript(string filePath, string name)
        {
            var script = File.ReadAllText(filePath);
            DSharpLexer lexer = new(script);
            lexer.Tokenize();
            AstParser parser = new(lexer);
            return parser.Parse(name);
        }
    }
}
