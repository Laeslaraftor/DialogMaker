using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Api
{
    internal unsafe class StandardLibraryExternalMethodsProvider(DSharpObjectsContainer objectsContainer) : IDSharpExternalMethodsProvider
    {
        private readonly DSharpObjectsContainer _objectsContainer = objectsContainer;
        private readonly Dictionary<IDSharpMethodInfo, DSharpExternalMethod?> _externalMethods = [];

        #region Controls

        public DSharpExternalMethod? GetMethod(IDSharpMethodInfo methodInfo)
        {
            if (_externalMethods.TryGetValue(methodInfo, out var result))
            {
                return result;
            }

            result = GetMethodImplementation(methodInfo);
            _externalMethods.Add(methodInfo, result);

            return result;
        }

        public DSharpExternalMethod? GetMethodImplementation(IDSharpMethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType.Namespace == "System" && methodInfo.DeclaringType.Name == "Array")
            {
                if (methodInfo.Name == "GetLength")
                {
                    return GetArrayLength;
                }
                else if (methodInfo.Name == "GetItem")
                {
                    return GetArrayItem;
                }
                else if (methodInfo.Name == "SetItem")
                {
                    return SetArrayItem;
                }
            }
            else if (methodInfo.DeclaringType.FullName == DSharpBuildInTypes.String)
            {
                if (methodInfo.Name == "GetLength")
                {
                    return GetArrayLength;
                }
                else if (methodInfo.Name == "GetValue")
                {
                    return GetStringValue;
                }
                else if (methodInfo.Name != "Ctor")
                {
                    return null;
                }

                var parameters = methodInfo.GetParameters();

                if (parameters.Length == 2 &&
                    parameters[0].Type.FullName == DSharpBuildInTypes.String &&
                    parameters[1].Type.FullName == DSharpBuildInTypes.String)
                {
                    return String2StringsCtorValue;
                }
                else if (parameters.Length == 1)
                {
                    if (parameters[0].Type.Namespace != "System" || parameters[0].Type.Name != "Array")
                    {
                        return null;
                    }
                    var genericParameters = parameters[0].Type.GetGenericParameters();

                    if (genericParameters.Length != 1)
                    {
                        return null;
                    }

                    var genericParameter = genericParameters[0];

                    if (genericParameter.FullName == DSharpBuildInTypes.String)
                    {
                        return StringStringsCtorValue;
                    }
                    else if (genericParameter.FullName == DSharpBuildInTypes.Char)
                    {
                        return StringCharsCtorValue;
                    }
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
                else if (methodInfo.Name == "ReadLine")
                {
                    return ConsoleReadLine;
                }
            }

            return null;
        }

        #endregion

        #region Array

        private static DSharpExternalMethodResult? GetArrayLength(DSharpObject* instance,
                                                                  DSharpRuntimeMethodInfo* methodInfo,
                                                                  UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                  UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            return DSharpArray.GetLength(instance);
        }
        private static DSharpExternalMethodResult? GetArrayItem(DSharpObject* instance,
                                                                DSharpRuntimeMethodInfo* methodInfo,
                                                                UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length == 0)
            {
                return DSharpExternalMethodResult.Null;
            }

            var indexArg = arguments[0];

            if (indexArg.Buffer.ValueType != DSharpStackValueType.Structure)
            {
                return DSharpExternalMethodResult.Null;
            }

            var index = DSharpObjectConverter.ToInt32((DSharpObject*)indexArg.Buffer.StackPointer);
            var array = (DSharpArray*)instance;
            var data = DSharpObject.GetData(instance);
            int itemSize = array->Size / array->Length;
            var item = (DSharpObject*)(data + itemSize * index);

            if (instance->Type->GenericParameters[0].AsPointer()->IsValueType)
            {
                return item;
            }

            return *(DSharpObject**)item;
        }
        private static DSharpExternalMethodResult? SetArrayItem(DSharpObject* instance,
                                                                DSharpRuntimeMethodInfo* methodInfo,
                                                                UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length < 2)
            {
                return null;
            }

            var indexArg = arguments[0];
            var valueArg = arguments[1];

            if (valueArg.Buffer.ValueType != DSharpStackValueType.Reference &&
                valueArg.Buffer.ValueType != DSharpStackValueType.Structure)
            {
                return null;
            }

            var index = DSharpObjectConverter.ToInt32((DSharpObject*)indexArg.Buffer.StackPointer);
            var array = (DSharpArray*)instance;
            var data = DSharpObject.GetData(instance);
            int itemSize = array->Size / array->Length;
            var item = (DSharpObject*)(data + itemSize * index);

            if (valueArg.Buffer.ValueType == DSharpStackValueType.Reference)
            {
                *(DSharpObject**)item = (DSharpObject*)valueArg.Buffer.ReadReference();
            }
            else
            {
                var value = (DSharpObject*)valueArg.Buffer.StackPointer;
                DSharpObject.Copy(value, item);
            }


            return null;
        }

        #endregion

        #region String

        private static DSharpExternalMethodResult? GetStringValue(DSharpObject* instance,
                                                                  DSharpRuntimeMethodInfo* methodInfo,
                                                                  UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                  UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length == 0)
            {
                return '\0';
            }

            var indexArg = arguments[0];
            var index = DSharpObjectConverter.ToInt32((DSharpObject*)indexArg.Buffer.StackPointer);
            var data = DSharpObject.GetData<char>(instance);

            return data[index];
        }
        private DSharpExternalMethodResult? StringCharsCtorValue(DSharpObject* instance,
                                                                 DSharpRuntimeMethodInfo* methodInfo,
                                                                 UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                 UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var charsObject = (DSharpObject*)arguments[0].Buffer.ReadReference();

            if (!charsObject->IsArray)
            {
                return DSharpExternalMethodResult.Null;
            }

            var charsArray = (DSharpArray*)charsObject;
            var data = DSharpObject.GetData<char>(charsObject);
            UnmanagedArray<char> values = new(data, charsArray->Length);

            return _objectsContainer.CreateString(values);
        }
        private DSharpExternalMethodResult? StringStringsCtorValue(DSharpObject* instance,
                                                                 DSharpRuntimeMethodInfo* methodInfo,
                                                                 UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                 UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var stringsObject = (DSharpObject*)arguments[0].Buffer.ReadReference();

            if (!stringsObject->IsArray)
            {
                return DSharpExternalMethodResult.Null;
            }

            var stringsArray = (DSharpArray*)stringsObject;
            var data = DSharpObject.GetData<Pointer<DSharpArray>>(stringsObject);
            int newStrLength = 0;

            for (int i = 0; i < stringsArray->Length; i++)
            {
                var value = data[i].AsPointer();
                newStrLength += value->Size;
            }

            var newStr = _objectsContainer.CreateString(newStrLength);
            var newStrData = DSharpObject.GetData<char>(newStr);

            for (int i = 0; i < stringsArray->Length; i++)
            {
                var value = data[i].AsPointer();
                var valueData = DSharpObject.GetData<char>((DSharpObject*)value);

                for (int c = 0; c < value->Length; c++)
                {
                    *newStrData = valueData[c];
                    newStrData++;
                }
            }

            return newStr;
        }
        private DSharpExternalMethodResult? String2StringsCtorValue(DSharpObject* instance,
                                                                    DSharpRuntimeMethodInfo* methodInfo,
                                                                    UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                    UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length != 2)
            {
                return DSharpExternalMethodResult.Null;
            }

            var str1 = (DSharpObject*)arguments[0].Buffer.ReadReference();
            var str2 = (DSharpObject*)arguments[1].Buffer.ReadReference();

            if (!str1->IsArray || !str2->IsArray)
            {
                return DSharpExternalMethodResult.Null;
            }

            var str1Array = (DSharpArray*)str1;
            var str2Array = (DSharpArray*)str1;
            var data1 = DSharpObject.GetData<char>(str1);
            var data2 = DSharpObject.GetData<char>(str2);
            var newStr = _objectsContainer.CreateString(str1Array->Length + str2Array->Length);
            var newStrData = DSharpObject.GetData<char>(newStr);

            for (int i = 0; i < str1Array->Length; i++)
            {
                newStrData[i] = data1[i];
            }

            newStrData += str1Array->Length;

            for (int i = 0; i < str2Array->Length; i++)
            {
                newStrData[i] = data2[i];
            }

            return newStr;
        }

        #endregion

        #region Console

        private static DSharpExternalMethodResult? ConsoleWrite(DSharpObject* instance,
                                                                DSharpRuntimeMethodInfo* methodInfo,
                                                                UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length == 1)
            {
                var textArg = arguments[0];
                var stringInstance = *(DSharpObject**)textArg.Buffer.StackPointer;
                char* chars = DSharpObject.GetData<char>(stringInstance);
                var length = DSharpArray.GetLength(stringInstance);

                for (int i = 0; i < length; i++)
                {
                    Console.Write(chars[i]);
                }
            }

            return null;
        }
        private static DSharpExternalMethodResult? ConsoleWriteLine(DSharpObject* instance,
                                                                    DSharpRuntimeMethodInfo* methodInfo,
                                                                    UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                    UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine();
                return null;
            }
            if (arguments[0].ParameterInfo->Type->Name == "Char")
            {
                return ConsoleWriteLineChar(instance, methodInfo, genericParameters, arguments);
            }

            ConsoleWrite(instance, methodInfo, genericParameters, arguments);
            Console.WriteLine();

            return null;
        }
        private static DSharpExternalMethodResult? ConsoleWriteLineChar(DSharpObject* instance,
                                                                        DSharpRuntimeMethodInfo* methodInfo,
                                                                        UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                        UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            var value = (DSharpObject*)arguments[0].Buffer.StackPointer;
            var data = *DSharpObject.GetData<char>(value);

            Console.WriteLine(data);

            return null;
        }
        private static DSharpExternalMethodResult? ConsoleReadLine(DSharpObject* instance,
                                                                   DSharpRuntimeMethodInfo* methodInfo,
                                                                   UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters,
                                                                   UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            return Console.ReadLine();
        }

        #endregion
    }
}
