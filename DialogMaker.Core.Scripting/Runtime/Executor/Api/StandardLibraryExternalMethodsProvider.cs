using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Diagnostics;

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
            var declaringType = methodInfo.DeclaringType;

            if (declaringType.Namespace == "System")
            {
                if (declaringType.Name == nameof(Array))
                {
                    if (methodInfo.Name == "GetLength")
                    {
                        return GetArrayLength;
                    }
                    else if (methodInfo.Name == "CanStackalloc")
                    {
                        return CanStackAlloc;
                    }
                }
                else if (declaringType.Name == "Numbers")
                {
                    if (methodInfo.Name == "Int64ToString")
                    {
                        return NumbersInt64ToString;
                    }
                    else if (methodInfo.Name == "UInt64ToString")
                    {
                        return NumbersUInt64ToString;
                    }
                    else if (methodInfo.Name == "DecimalToString")
                    {
                        return NumbersDecimalToString;
                    }
                }
                else if (declaringType.Name == "Object")
                {
                    if (methodInfo.Name == "ReferenceEquals")
                    {
                        return ObjectReferenceEquals;
                    }
                    else if (methodInfo.Name == "ContentEquals")
                    {
                        return ObjectContentEquals;
                    }
                    else if (methodInfo.Name == "GetHashCode")
                    {
                        return ObjectGetHashCode;
                    }
                }
                else if (declaringType.FullName == DSharpBuildInTypes.String)
                {
                    if (methodInfo.Name == "GetLength")
                    {
                        return GetArrayLength;
                    }
                    else if (methodInfo.Name != "Ctor")
                    {
                        return null;
                    }

                    var parameters = methodInfo.GetParameters();
                    var firstParameter = parameters[0];

                    if (parameters.Length == 2 &&
                        firstParameter.Type.FullName == DSharpBuildInTypes.String &&
                        parameters[1].Type.FullName == DSharpBuildInTypes.String)
                    {
                        return String2StringsCtorValue;
                    }
                    else if (parameters.Length == 1)
                    {
                        if (firstParameter.Type.Namespace != "System")
                        {
                            return null;
                        }
                        else if (parameters[0].Type.Name == "Span")
                        {
                            return StringCharsSpanCtorValue;
                        }
                        else if (parameters[0].Type.Name != "Array")
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
                            return StringCharsArrayCtorValue;
                        }
                    }
                }
                else if (declaringType.Name == nameof(Console))
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
                    else if (methodInfo.Name == "Clear")
                    {
                        return ConsoleClear;
                    }
                }
                else if (declaringType.Name == nameof(Environment))
                {
                    if (methodInfo.Name == "GetTickCount")
                    {
                        return EnvironmentGetTickCount;
                    }
                    else if (methodInfo.Name == "GetNewLine")
                    {
                        return EnvironmentGetNewLine;
                    }
                }
                else if (declaringType.Name == nameof(BitConverter))
                {
                    if (methodInfo.Name == "GetIsLittleEndian")
                    {
                        return GetIsLittleEndian;
                    }
                }
            }
            else if (declaringType.Namespace == "System.Dotnet")
            {
                if (declaringType.Name == "DotnetObject")
                {
                    if (methodInfo.Name == nameof(CreateDotnetObject))
                    {
                        return CreateDotnetObject;
                    }
                    else if (methodInfo.Name == nameof(DisposeDotnetObject))
                    {
                        return DisposeDotnetObject;
                    }
                }
            }
            else if (declaringType.Namespace == "System.Diagnostics")
            {
                if (declaringType.Name == nameof(Stopwatch))
                {
                    if (methodInfo.Name == "GetFrequency")
                    {
                        return StopwatchGetFrequency;
                    }
                    else if (methodInfo.Name == "GetIsHighResolution")
                    {
                        return StopwatchGetIsHighResolution;
                    }
                    else if (methodInfo.Name == "GetTimestamp")
                    {
                        return StopwatchGetTimestamp;
                    }
                }
            }
            else if (declaringType.Namespace == "System.Threading")
            {
                if (declaringType.Name == nameof(Monitor))
                {
                    if (methodInfo.Name == "Enter")
                    {
                        return MonitorEnter;
                    }
                    else if (methodInfo.Name == "Exit")
                    {
                        return MonitorExit;
                    }
                }
            }
            else if (declaringType.Namespace == "Internal.System.Runtime")
            {
                if (declaringType.Name == "CompilerServices")
                {
                    if (methodInfo.Name == "GetObjectAddress")
                    {
                        return CompilerServicesGetObjectAddress;
                    }
                    else if (methodInfo.Name == "GetObjectContentAddress")
                    {
                        return CompilerServicesGetObjectContentAddress;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Array

        private static DSharpExternalMethodResult? GetArrayLength(DSharpExternalCallingArgs args)
        {
            return DSharpArray.GetLength(args.Instance);
        }
        private static DSharpExternalMethodResult? CanStackAlloc(DSharpExternalCallingArgs args)
        {
            if (args.GenericParameter.Count == 0 || args.Arguments.Length == 0)
            {
                return false;
            }

            var valueType = (DSharpRuntimeTypeInfo*)args.GenericParameter[0].Value.AsPointer();
            var lengthArgument = args.Arguments[0].Buffer.ReadAsObject();

            if (lengthArgument == null)
            {
                return false;
            }

            var length = DSharpObjectConverter.ToInt32(lengthArgument);

            return length >= 0 && args.Stack.AvailableSize > length * valueType->ItemSize;
        }

        #endregion

        #region String

        private DSharpExternalMethodResult? StringCharsArrayCtorValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var charsObject = arguments[0].Buffer.ReadAsObject();

            if (!charsObject->IsArray)
            {
                return DSharpExternalMethodResult.Null;
            }

            return CreateStringFromCharsArray(args, (DSharpArray*)charsObject);
        }
        private DSharpExternalMethodResult? StringCharsSpanCtorValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var charsObject = arguments[0].Buffer.ReadAsObject();

            if (charsObject == null)
            {
                return DSharpExternalMethodResult.Null;
            }

            if (charsObject->Type->TryGetField("_items", out var itemsField))
            {
                int size = itemsField->FieldType->ItemSize;
                byte* stackBuffer = stackalloc byte[size];
                UnmanagedArray<byte> buffer = new(stackBuffer, size);
                var value = itemsField->Read(charsObject, buffer);

                if (value != null)
                {
                    return CreateStringFromCharsArray(args, (DSharpArray*)value);
                }
            }

            if (charsObject->Type->TryGetField("_itemsPointer", out var itemsPointerField) &&
                charsObject->Type->TryGetField("_length", out var lengthField))
            {
                var itemsPointer = itemsPointerField->Read<nint>(charsObject);
                var length = lengthField->Read<int>(charsObject);

                return _objectsContainer.CreateString((char*)itemsPointer, length);
            }

            throw new InvalidOperationException($"Unable to find items pointer and length fields at \"{charsObject->Type->ToString()}\"");
        }
        private DSharpExternalMethodResult? StringStringsCtorValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

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
        private DSharpExternalMethodResult? String2StringsCtorValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 2)
            {
                return DSharpExternalMethodResult.Null;
            }

            var str1 = (DSharpObject*)arguments[0].Buffer.ReadReference();
            var str2 = (DSharpObject*)arguments[1].Buffer.ReadReference();

            if (str1 == null && str2 != null)
            {
                return str2;
            }
            else if (str2 == null && str1 != null)
            {
                return str1;
            }
            else if (str1 == null && str2 == null)
            {
                return DSharpExternalMethodResult.Null;
            }
            if (!str1->IsArray || !str2->IsArray)
            {
                return DSharpExternalMethodResult.Null;
            }

            var str1Array = (DSharpArray*)str1;
            var str2Array = (DSharpArray*)str2;
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
        private DSharpExternalMethodResult? CreateStringFromCharsArray(DSharpExternalCallingArgs args, DSharpArray* charsArray)
        {
            if (charsArray == null)
            {
                return DSharpExternalMethodResult.Null;
            }

            char* chars = stackalloc char[charsArray->Length];
            UnmanagedArray<char> values = new(chars, charsArray->Length);
            var indexer = DSharpArray.GetIndexer<char>(charsArray);

            for (int i = 0; i < charsArray->Length; i++)
            {
                values[i] = indexer[i];
            }

            return _objectsContainer.CreateString(values);
        }

        #endregion

        #region Object

        private static DSharpExternalMethodResult? ObjectReferenceEquals(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 2)
            {
                return false;
            }

            var objectArg1 = arguments[0];
            var objectArg2 = arguments[1];

            if (objectArg1.Buffer.ValueType != DSharpStackValueType.Reference ||
                objectArg2.Buffer.ValueType != DSharpStackValueType.Reference)
            {
                return false;
            }

            return objectArg1.Buffer.ReadReference() == objectArg2.Buffer.ReadReference();
        }
        private static DSharpExternalMethodResult? ObjectContentEquals(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 2)
            {
                return false;
            }

            var objectArg1 = arguments[0];
            var objectArg2 = arguments[1];
            var a = objectArg1.Buffer.ReadAsObject();
            var b = objectArg2.Buffer.ReadAsObject();

            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }

            var aSize = DSharpObject.GetSize(a);
            var bSize = DSharpObject.GetSize(b);

            if (aSize != bSize)
            {
                return false;
            }

            byte* aData = DSharpObject.GetData(a);
            byte* bData = DSharpObject.GetData(b);

            return RuntimeExtensions.Equals(aData, bData, aSize);
        }
        private static DSharpExternalMethodResult? ObjectGetHashCode(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return 0;
            }

            var objectArg = arguments[0];
            DSharpObject* instance = objectArg.Buffer.ReadAsObject();

            return GetHashCode(instance);
        }

        private static int GetHashCode(DSharpObject* instance)
        {
            if (instance == null)
            {
                return 0;
            }
            if (!instance->Type->IsValueType)
            {
                return (int)instance / (int)instance->Type;
            }
            if (instance->Type->BuildInValueTypeIndex != -1)
            {
                return DSharpObjectConverter.ToObject(instance).GetHashCode();
            }

            int sum = 0;
            int currentStackBufferSize = 512;
            byte* stackBuffer = stackalloc byte[currentStackBufferSize];

            goto SumFields;
        SumFields:
            sum = 0;

            for (int i = 0; i < instance->Type->Fields.Length; i++)
            {
                var field = instance->Type->Fields[i];

                if (field.IsStatic)
                {
                    continue;
                }

                var size = field.FieldType->ItemSize;

                if (size > currentStackBufferSize)
                {
                    currentStackBufferSize = size;
                    goto IncrementBuffer;
                }

                UnmanagedArray<byte> buffer = new(stackBuffer, size);

                var value = field.Read(instance, buffer);

                sum += GetHashCode(value);
            }

            goto Complete;
        IncrementBuffer:
            byte* newStackBuffer = stackalloc byte[currentStackBufferSize];
            stackBuffer = newStackBuffer;
            goto SumFields;

        Complete:
            return sum;
        }

        #endregion

        #region CompilerServices

        private static DSharpExternalMethodResult? CompilerServicesGetObjectContentAddress(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;
            var valueType = (DSharpRuntimeTypeInfo*)args.GenericParameter[2].Value.AsPointer();

            if (arguments.Length != 1)
            {
                args.Stack.PushStructure(valueType, IntPtr.Zero);
                return DSharpExternalMethodResult.Stack;
            }

            var objectArg = arguments[0].Buffer.ReadAsObject();

            if (objectArg == null)
            {
                args.Stack.PushStructure(valueType, IntPtr.Zero);
                return DSharpExternalMethodResult.Stack;
            }

            var objectDataAddress = (nint)DSharpObject.GetData(objectArg);
            args.Stack.PushStructure(valueType, objectDataAddress);

            return DSharpExternalMethodResult.Stack;
        }
        private static DSharpExternalMethodResult? CompilerServicesGetObjectAddress(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return IntPtr.Zero;
            }

            var objectArg = arguments[0].Buffer.ReadAsObject();

            return (nint)objectArg;
        }

        #endregion

        #region Numbers

        private DSharpExternalMethodResult? NumbersInt64ToString(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var value = arguments[0].Buffer.ReadAsObject();
            var longValue = decimal.ToInt64(DSharpObjectConverter.ToObject<decimal>(value));

            return _objectsContainer.CreateString(longValue.ToString());
        }
        private DSharpExternalMethodResult? NumbersUInt64ToString(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var value = arguments[0].Buffer.ReadAsObject();
            var ulongValue = DSharpObjectConverter.ToObject<ulong>(value);

            return _objectsContainer.CreateString(ulongValue.ToString());
        }
        private DSharpExternalMethodResult? NumbersDecimalToString(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var value = arguments[0].Buffer.ReadAsObject();
            var decimalValue = DSharpObjectConverter.ToObject<decimal>(value);

            return _objectsContainer.CreateString(decimalValue.ToString());
        }

        #endregion

        #region Console

        private static DSharpExternalMethodResult? ConsoleWrite(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length == 1)
            {
                var textArg = arguments[0];
                var stringInstance = textArg.Buffer.ReadAsObject();

                if (stringInstance == null)
                {
                    return DSharpExternalMethodResult.Null;
                }

                char* chars = DSharpObject.GetData<char>(stringInstance);
                var length = DSharpArray.GetLength(stringInstance);

                for (int i = 0; i < length; i++)
                {
                    Console.Write(chars[i]);
                }
            }

            return null;
        }
        private static DSharpExternalMethodResult? ConsoleWriteLine(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length == 0)
            {
                Console.WriteLine();
                return null;
            }
            if (arguments[0].ParameterInfo.Type->Name == "Char")
            {
                return ConsoleWriteLineChar(args);
            }

            ConsoleWrite(args);
            Console.WriteLine();

            return null;
        }
        private static DSharpExternalMethodResult? ConsoleWriteLineChar(DSharpExternalCallingArgs args)
        {
            var value = (DSharpObject*)args.Arguments[0].Buffer.StackPointer;
            var data = *DSharpObject.GetData<char>(value);

            Console.WriteLine(data);

            return null;
        }
        private static DSharpExternalMethodResult? ConsoleReadLine(DSharpExternalCallingArgs args)
        {
            return Console.ReadLine();
        }
        private static DSharpExternalMethodResult? ConsoleClear(DSharpExternalCallingArgs args)
        {
            Console.Clear();
            return null;
        }

        #endregion

        #region Envinronment

        private static DSharpExternalMethodResult? EnvironmentGetNewLine(DSharpExternalCallingArgs _)
        {
            return Environment.NewLine;
        }
        private static DSharpExternalMethodResult? EnvironmentGetTickCount(DSharpExternalCallingArgs _)
        {
            return Environment.TickCount;
        }

        #endregion

        #region Monitor

        private DSharpExternalMethodResult? MonitorEnter(DSharpExternalCallingArgs args)
        {
            if (args.Arguments.Length == 0)
            {
                return null;
            }

            var obj = args.Arguments[0].Buffer.ReadAsObject();
            _objectsContainer.Lock(obj);

            return null;
        }
        private DSharpExternalMethodResult? MonitorExit(DSharpExternalCallingArgs args)
        {
            if (args.Arguments.Length == 0)
            {
                return null;
            }

            var obj = args.Arguments[0].Buffer.ReadAsObject();
            _objectsContainer.Unlock(obj);

            return null;
        }

        #endregion

        #region BitConverter

        private static DSharpExternalMethodResult? GetIsLittleEndian(DSharpExternalCallingArgs _)
        {
            return BitConverter.IsLittleEndian;
        }

        #endregion

        #region Stopwatch

        private static DSharpExternalMethodResult? StopwatchGetFrequency(DSharpExternalCallingArgs _)
        {
            return Stopwatch.Frequency;
        }
        private static DSharpExternalMethodResult? StopwatchGetIsHighResolution(DSharpExternalCallingArgs _)
        {
            return Stopwatch.IsHighResolution;
        }
        private static DSharpExternalMethodResult? StopwatchGetTimestamp(DSharpExternalCallingArgs _)
        {
            return Stopwatch.GetTimestamp();
        }

        #endregion

        #region Dotnet wrappers

        private DSharpExternalMethodResult? CreateDotnetObject(DSharpExternalCallingArgs args)
        {
            if (args.Arguments.Length != 2)
            {
                return false;
            }

            var handlerInstance = args.Arguments[0].Buffer.ReadAsObject();
            var typeNameInstance = args.Arguments[1].Buffer.ReadAsObject();

            if (handlerInstance == null || typeNameInstance == null)
            {
                return false;
            }

            var typeName = DSharpObjectConverter.ToString(typeNameInstance);

            if (_objectsContainer.CreateDotnetWrapper(typeName, out var wrapper))
            {
                handlerInstance->DotnetId = wrapper.Id;
                return true;
            }

            return false;
        }
        private DSharpExternalMethodResult? DisposeDotnetObject(DSharpExternalCallingArgs args)
        {
            if (args.Arguments.Length != 1)
            {
                return null;
            }

            var instance = args.Arguments[0].Buffer.ReadAsObject();
            _objectsContainer.DisposeDotnetWrapper(instance->DotnetId);

            return null;
        }

        #endregion
    }
}
