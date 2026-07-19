using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Api
{
    internal unsafe class StandardLibraryExternalMethodsProvider : IDSharpExternalMethodsProvider
    {
        public DSharpExternalMethod? GetMethod(IDSharpMethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType.FullName == DSharpBuildInTypes.String)
            {
                if (methodInfo.Name == "GetLength")
                {
                    return GetStringLength;
                }
                else if (methodInfo.Name == "GetValue")
                {
                    return GetStringValue;
                }
            }
            if (methodInfo.DeclaringType.FullName == "System.Console")
            {
                if (methodInfo.Name == "WriteLine")
                {
                    return ConsoleWriteLine;
                }
                else if (methodInfo.Name == "Write")
                {
                    return ConsoleWrite;
                }
            }

            return null;
        }

        #region String

        private static DSharpLiteralValue? GetStringLength(DSharpObject* instance,
                                                           DSharpRuntimeMethodInfo* methodInfo,
                                                           UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                           UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            return instance->Length;
        }
        private static DSharpLiteralValue? GetStringValue(DSharpObject* instance,
                                                          DSharpRuntimeMethodInfo* methodInfo,
                                                          UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                          UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length == 0)
            {
                return '\0';
            }

            var indexArg = arguments[0];

            if (indexArg.Buffer.Size != sizeof(int))
            {
                return '\0';
            }

            var index = DSharpObjectConverter.ToInt32((DSharpObject*)indexArg.Buffer.StackPointer);

            return ((char*)(instance + sizeof(DSharpObject)))[index];
        }

        #endregion

        #region Console

        private static DSharpLiteralValue? ConsoleWrite(DSharpObject* instance,
                                                           DSharpRuntimeMethodInfo* methodInfo,
                                                           UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                           UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length == 1)
            {
                var textArg = arguments[0];
                var stringInstance = *(DSharpObject**)textArg.Buffer.StackPointer;
                char* chars = (char*)stringInstance + sizeof(DSharpObject);

                for (int i = 0; i < stringInstance->Length; i++)
                {
                    Console.Write(chars[i]);
                }
            }

            return null;
        }
        private static DSharpLiteralValue? ConsoleWriteLine(DSharpObject* instance,
                                                            DSharpRuntimeMethodInfo* methodInfo,
                                                            UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                            UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            ConsoleWrite(instance, methodInfo, genericParameters, arguments);
            Console.WriteLine();

            return null;
        }

        #endregion

        #region Static

        public static readonly StandardLibraryExternalMethodsProvider Instance = new();

        #endregion
    }
}
