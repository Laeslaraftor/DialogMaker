using DialogMaker.Core.Scripting.Compiler;
using DialogMaker.Core.Scripting.Compiler.Builders;
using System.Diagnostics;

namespace DialogMaker.Core.Tests
{
    internal class ScriptBytecodeTests
    {
        [Test]
        [TestCase(ScriptCompilerTests.SimpleScript, "repeat")]
        [TestCase(ScriptCompilerTests.SimpleScript, "sum")]
        [TestCase(ScriptCompilerTests.SimpleScript, "getTextColor")]
        [TestCase(ScriptCompilerTests.SimpleScript, "getGenericValue")]
        [TestCase(ScriptCompilerTests.SimpleScript, "getLines")]
        [TestCase(ScriptCompilerTests.SimpleScript, "foreachTest")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.Threading.Thread.Increment")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.Double.GetSquared")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.List`1.Add")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.List`1.Remove")]
        [TestCase(ScriptCompilerTests.SimpleScript, "System.List`1.Expand")]
        [TestCase(ScriptCompilerTests.TypeScript, "System.String.GetFirstSymbol")]
        [TestCase(ScriptCompilerTests.TypeScript, "Character.PrintName")]
        [TestCase(ScriptCompilerTests.TypeScript, "Player.ToString")]
        [TestCase(ScriptCompilerTests.TypeScript, "Player.SetValues")]
        [TestCase(ScriptCompilerTests.TypeScript, "Player.GetValues")]
        [TestCase(ScriptCompilerTests.MathScript, "globalFunction")]
        [TestCase(ScriptCompilerTests.OperatorsScript, "castTypes")]
        [TestCase(ScriptCompilerTests.OperatorsScript, "sumTypes")]
        [TestCase(ScriptCompilerTests.OperatorsScript, "unaryTest")]
        [TestCase(ScriptCompilerTests.OperatorsScript, "customBinaryOperatorWithAssignment")]
        [TestCase(ScriptCompilerTests.GenericMethodsScript, "GetHudoeName")]
        [TestCase(ScriptCompilerTests.GenericMethodsScript, "MegaClass.CreateInstance")]
        [TestCase(ScriptCompilerTests.TryCatchFinallyScript, "CatchException")]
        public static void PrintSimpleFunctionBytecode(string scriptName, string functionName)
        {
            var assembly = ScriptCompilerTests.CompileScript(scriptName);
            ReadFunctionOrMethod(assembly, functionName);
        }

        private static void ReadFunctionOrMethod(DSharpAssemblyBuilder assembly, string fullName)
        {
            string[] parts = fullName.Split('.');

            if (parts.Length == 1)
            {
                ReadFunction(assembly, parts[0]);
            }
            else if (parts.Length > 1)
            {
                var typeName = fullName.Replace("." + parts[^1], string.Empty);
                var type = assembly.GetType(typeName);
                
                if (type is DSharpTypeBuilder builder)
                {
                    ReadMethod(builder, parts[^1]);
                }
                else
                {
                    Debug.Fail($"Type \"{type}\" is not builder");
                }
            }
            else
            {
                Debug.Fail($"Invalid function or method name \"{fullName}\"");
            }
        }
        private static void ReadFunction(DSharpAssemblyBuilder assembly, string functionName)
        {
            var function = assembly.GlobalFunctions.FirstOrDefault(f => f.Name == functionName);

            if (function == null)
            {
                Debug.Fail($"Unable to find function \"{functionName}\"");
                return;
            }

            ReadCode(function);
        }
        private static void ReadMethod(DSharpTypeBuilder type, string methodName)
        {
            var method = type.Methods.FirstOrDefault(f => f.Name == methodName);

            if (method == null)
            {
                Debug.Fail($"Unable to find method \"{methodName}\" at \"{type}\"");
                return;
            }

            ReadCode(method);
        }
        private static void ReadCode(DSharpMethodBuilder method)
        {
            var code = method.GetBytecodeBuilder();
            Console.WriteLine("Raw bytecode:");
            Console.WriteLine(code.ToString());

            DSharpBytecodeOptimizer.Optimize(method.Assembly);

            Console.WriteLine();
            Console.WriteLine("Optimized:");
            Console.WriteLine(code.ToString());
        }
    }
}
