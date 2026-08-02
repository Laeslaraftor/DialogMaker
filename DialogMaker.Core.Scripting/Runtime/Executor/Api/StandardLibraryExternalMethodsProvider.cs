using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

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
            if (methodInfo.DeclaringType.Namespace == "System")
            {
                if (methodInfo.DeclaringType.Name == "Array")
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
                else if (methodInfo.DeclaringType.Name == "Span")
                {
                    if (methodInfo.Name == "GetValue")
                    {
                        return GetSpanItem;
                    }
                    else if (methodInfo.Name == "SetValue")
                    {
                        return SetSpanItem;
                    }
                }
                else if (methodInfo.DeclaringType.Name == "Numbers")
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
                else if (methodInfo.DeclaringType.Name == "Object")
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
                else if (methodInfo.DeclaringType.FullName == "System.Console")
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
            }
            else if (methodInfo.DeclaringType.Namespace == "System.Native")
            {
                if (methodInfo.DeclaringType.Name == "Pointer")
                {
                    if (methodInfo.Name == "ReadValue")
                    {
                        return PointerReadValue;
                    }
                    else if (methodInfo.Name == "WriteValue")
                    {
                        return PointerWriteValue;
                    }
                }
            }
            else if (methodInfo.DeclaringType.Namespace == "Internal.System.Runtime")
            {
                if (methodInfo.DeclaringType.Name == "CompilerServices")
                {
                    if (methodInfo.Name == "GetObjectAddress")
                    {
                        return CompilerServicesGetObjectAddress;
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
        private static DSharpExternalMethodResult? GetArrayItem(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length == 0)
            {
                return DSharpExternalMethodResult.Null;
            }

            var indexArg = arguments[0];

            if (indexArg.Buffer.ValueType != DSharpStackValueType.Structure)
            {
                return DSharpExternalMethodResult.Null;
            }

            var index = DSharpObjectConverter.ToInt32(indexArg.Buffer.ReadAsObject());
            var array = (DSharpArray*)args.Instance;
            var item = DSharpArray.GetItem(array, index);

            if (!array->ItemsType->IsValueType)
            {
                return *(DSharpObject**)item;
            }

            args.Stack.PushStructure(array->ItemsType, new UnmanagedArray<byte>(item, array->ItemSize));

            return null;
        }
        private static DSharpExternalMethodResult? SetArrayItem(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length < 2)
            {
                return null;
            }

            var indexArg = arguments[0];
            var valueArg = arguments[1];

            if (valueArg.Buffer.ValueType != DSharpStackValueType.Reference &&
                valueArg.Buffer.ValueType != DSharpStackValueType.Structure &&
                valueArg.Buffer.ValueType != DSharpStackValueType.Null)
            {
                return null;
            }

            var index = DSharpObjectConverter.ToInt32((DSharpObject*)indexArg.Buffer.StackPointer);
            var array = (DSharpArray*)args.Instance;
            var item = DSharpArray.GetItem(array, index);
            var value = valueArg.Buffer.ReadAsObject();

            if (valueArg.Buffer.ValueType == DSharpStackValueType.Reference)
            {
                var oldValue = *(DSharpObject**)item;

                if (oldValue != null)
                {
                    oldValue->ReferencesCount--;
                }
                if (value != null)
                {
                    value->ReferencesCount++;
                }

                *(DSharpObject**)item = value;
            }
            else
            {
                UnmanagedArray<byte> buffer = new(item, array->ItemSize);
                DSharpObject.CopyData(value, buffer);
            }

            return null;
        }

        #endregion

        #region Span

        private static DSharpExternalMethodResult? GetSpanItem(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 2)
            {
                return DSharpExternalMethodResult.Null;
            }

            var itemsArg = arguments[0];
            var indexArg = arguments[1];

            if (indexArg.Buffer.ValueType != DSharpStackValueType.Structure &&
                itemsArg.Buffer.ValueType != DSharpStackValueType.Structure)
            {
                return DSharpExternalMethodResult.Null;
            }

            var index = DSharpObjectConverter.ToInt32(indexArg.Buffer.ReadAsObject());
            var items = (void*)DSharpObjectConverter.ToIntPtr(itemsArg.Buffer.ReadAsObject());
            var genericParameters = args.Instance->Type->GenericParameters;

            if (genericParameters.Length == 0)
            {
                return DSharpExternalMethodResult.Null;
            }

            var itemsType = genericParameters[0].AsPointer();
            var itemSize = itemsType->ItemSize;
            var item = DSharpArray.GetItem(items, itemSize, index);

            if (!itemsType->IsValueType)
            {
                return *(DSharpObject**)item;
            }

            args.Stack.PushStructure(itemsType, new UnmanagedArray<byte>(item, itemSize));

            return null;
        }
        private static DSharpExternalMethodResult? SetSpanItem(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 3)
            {
                return null;
            }

            var itemsArg = arguments[0];
            var indexArg = arguments[1];
            var valueArg = arguments[2];

            if (valueArg.Buffer.ValueType != DSharpStackValueType.Reference &&
                valueArg.Buffer.ValueType != DSharpStackValueType.Structure &&
                valueArg.Buffer.ValueType != DSharpStackValueType.Null)
            {
                return null;
            }

            var index = DSharpObjectConverter.ToInt32((DSharpObject*)indexArg.Buffer.StackPointer);
            var items = (void*)DSharpObjectConverter.ToIntPtr((DSharpObject*)itemsArg.Buffer.StackPointer);
            var genericParameters = args.Instance->Type->GenericParameters;

            if (genericParameters.Length == 0)
            {
                return DSharpExternalMethodResult.Null;
            }

            var itemsType = genericParameters[0].AsPointer();
            var itemSize = itemsType->ItemSize;
            var item = DSharpArray.GetItem(items, itemSize, index);
            var value = valueArg.Buffer.ReadAsObject();

            if (valueArg.Buffer.ValueType == DSharpStackValueType.Reference)
            {
                var oldValue = *(DSharpObject**)item;

                if (oldValue != null)
                {
                    oldValue->ReferencesCount--;
                }
                if (value != null)
                {
                    value->ReferencesCount++;
                }

                *(DSharpObject**)item = value;
            }
            else
            {
                UnmanagedArray<byte> buffer = new(item, itemSize);
                DSharpObject.CopyData(value, buffer);
            }

            return null;
        }

        #endregion

        #region String

        private static DSharpExternalMethodResult? GetStringValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length == 0)
            {
                return '\0';
            }

            var indexArg = arguments[0];
            var index = DSharpObjectConverter.ToInt32(indexArg.Buffer.ReadAsObject());
            var data = DSharpObject.GetData<char>(args.Instance);

            return data[index];
        }
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

            var aLength = DSharpArray.GetLength(a);
            var bLength = DSharpArray.GetLength(b);

            if (aLength != bLength)
            {
                return false;
            }

            var aData = DSharpObject.GetData(a);
            var bData = DSharpObject.GetData(b);

            for (int i = 0; i < aLength; i++)
            {
                if (aData[i] != bData[i])
                {
                    return false;
                }
            }

            return true;
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

        #region Pointer

        private static DSharpExternalMethodResult? PointerReadValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 1 ||
                args.GenericParameter.Count != 1)
            {
                return DSharpExternalMethodResult.Null;
            }

            var addressArg = arguments[0];
            var resultType = args.GenericParameter[0].Value.AsPointer();
            var addressObject = addressArg.Buffer.ReadAsObject();

            if (addressObject == null)
            {
                return DSharpExternalMethodResult.Null;
            }

            nint address = DSharpObjectConverter.ToIntPtr(addressObject);
            var resultFrame = args.Stack.PushStructure(resultType);
            var resultObject = resultFrame.ReadAsObject();

            void* objectData = DSharpObject.GetData(resultObject);
            var size = resultType->Size;
            var typeInfo = (DSharpRuntimeTypeInfo*)address;

            Buffer.MemoryCopy((void*)address, objectData, size, size);

            return DSharpExternalMethodResult.Stack;
        }
        private static DSharpExternalMethodResult? PointerWriteValue(DSharpExternalCallingArgs args)
        {
            var arguments = args.Arguments;

            if (arguments.Length != 2)
            {
                return null;
            }

            var addressArg = arguments[0];
            var valueArg = arguments[1];
            var addressObject = addressArg.Buffer.ReadAsObject();
            var valueObject = valueArg.Buffer.ReadAsObject();

            if (addressObject == null || valueObject == null)
            {
                return null;
            }

            nint address = DSharpObjectConverter.ToIntPtr(addressObject);

            void* objectData = DSharpObject.GetData(valueObject);
            var size = valueObject->Type->Size;

            Buffer.MemoryCopy(objectData, (void*)address, size, size);

            return null;
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
            var longValue = DSharpObjectConverter.ToObject<long>(value);

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

        #endregion
    }
}
