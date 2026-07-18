using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Executor;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.ScriptingExample
{
    public unsafe class StandardExternalMethodsProvider : IDSharpExternalMethodsProvider
    {
        public DSharpExternalMethod? GetMethod(IDSharpMethodInfo methodInfo)
        {
            if (methodInfo.Name == "WriteLine" && methodInfo.DeclaringType.Name == "Console")
            {
                return ConsoleWriteLine;
            }

            throw new NotImplementedException();
        }

        private static DSharpLiteralValue? ConsoleWriteLine(DSharpObject* instance,
                                                            DSharpRuntimeMethodInfo* methodInfo,
                                                            UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                            UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length > 0)
            {
                var textArg = arguments[0];
                var stringInstance = *(DSharpObject**)textArg.Buffer.StackPointer;
                string text = DSharpObject.ToString(stringInstance);

                Console.WriteLine(text);
            }

            return null;
        }
    }
}
