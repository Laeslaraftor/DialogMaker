using System.Diagnostics;

namespace DialogMaker.Core.Tests
{
    internal class ScriptBytecodeTests
    {
        [Test, TestCase("repeat"), TestCase("getValue")]
        public static void PrintSimpleFunctionBytecode(string functionName)
        {
            ReadFunction(functionName);
        }

        private static void ReadFunction(string functionName)
        {
            var assembly = ScriptCompilerTests.CompileSimpleScript();
            var function = assembly.GlobalFunctions.FirstOrDefault(f => f.Name == functionName);

            if (function == null)
            {
                Debug.Fail($"Unable to find function \"{functionName}\"");
                return;
            }

            var code = function.GetBytecodeBuilder();

            foreach (var instruction in code.Instructions)
            {
                Console.WriteLine(instruction.ToString());
            }
        }
    }
}
