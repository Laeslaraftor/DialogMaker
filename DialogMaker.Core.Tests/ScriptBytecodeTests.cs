using DialogMaker.Core.Scripting.Runtime.Builders;
using DialogMaker.Core.Scripting.Runtime.Compilers;
using System.Diagnostics;

namespace DialogMaker.Core.Tests
{
    internal class ScriptBytecodeTests
    {
        [Test]
        [TestCase("repeat")]
        [TestCase("sum")]
        [TestCase("getTextColor")]
        [TestCase("getNumberNameType")]
        [TestCase("getGenericValue")]
        [TestCase("getLines")]
        [TestCase("foreachTest")]
        [TestCase("System.Number.GetSquared")]
        [TestCase("System.List`1.Add")]
        [TestCase("System.List`1.Expand")]
        public static void PrintSimpleFunctionBytecode(string functionName)
        {
            var assembly = ScriptCompilerTests.CompileSimpleScript();
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
