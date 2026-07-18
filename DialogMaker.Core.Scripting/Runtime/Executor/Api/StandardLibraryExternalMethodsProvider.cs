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

            var index = indexArg.Buffer.Read<int>();

            return ((char*)(instance + sizeof(DSharpObject)))[index];
        }

        #endregion

        #region Static

        public static readonly StandardLibraryExternalMethodsProvider Instance = new();

        #endregion
    }
}
