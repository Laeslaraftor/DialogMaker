using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Objects instances container
    /// </summary>
    public unsafe class DSharpObjectsContainer(IDSharpAssembly assembly, DSharpVmMemoryManager memoryManager, DSharpRuntimeInformationProvider runtimeInformationProvider) : Disposable
    {
        /// <summary>
        /// D# assembly
        /// </summary>
        public IDSharpAssembly Assembly { get; } = assembly;

        private readonly List<nint> _objects = [];
        private readonly DSharpRuntimeInformationProvider _runtimeInformationProvider = runtimeInformationProvider;
        private readonly DSharpVmMemoryManager _memoryManager = memoryManager;

        #region Controls

        /// <summary>
        /// Create new instance of object with specified type
        /// </summary>
        /// <param name="type">Type of new instance</param>
        /// <returns>New instance of D# object</returns>
        public DSharpObject* Create(DSharpRuntimeTypeInfo* type) => Create(type, type->Size);
        /// <summary>
        /// Create new instance of array with specified type
        /// </summary>
        /// <param name="type">Type of new array</param>
        /// <param name="length">Array length</param>
        /// <param name="stack">Stack for creating array at stack (for span)</param>
        /// <returns>New instance of D# array</returns>
        public DSharpObject* CreateArray(DSharpRuntimeTypeInfo* type, int length, DSharpStack? stack)
        {
            var itemType = (DSharpRuntimeTypeInfo*)type->GenericParameters[0];
            var itemsSize = itemType->ItemSize;
            var size = itemsSize * length;
            DSharpObject* obj;

            if (type->IsValueType)
            {
                if (stack == null)
                {
                    throw new ArgumentException($"Stack not provided for creating span");
                }

                var buffer = stack.AllocateBuffer(size);
                var spanFrame = stack.PushStructureRef(type, true);

                buffer->FrameInfo = spanFrame;
                spanFrame->Buffer = buffer;
                obj = (DSharpObject*)spanFrame->StackPointer;

                Setup(obj, DSharpObjectAttributes.StoredInBuffer, type, false);

                if (!type->TryGetField("_itemsPointer", out var itemsPointerField) ||
                    !type->TryGetField("_length", out var lengthField) ||
                    !type->TryGetField("_items", out var itemsField))
                {
                    throw new InvalidOperationException($"Unable to find items pointer and/or length field at \"{type->ToString()}\"");
                }

                itemsField->Write(this, obj, null);

                stack.Push(buffer->StackPointer);
                itemsPointerField->Write(this, obj, stack, 0);
                stack.Pop();

                stack.Push(length);
                lengthField->Write(this, obj, stack, 0);
                stack.Pop();

                return obj;
            }
            else
            {
                obj = Create(type, size, true);
            }

            var array = (DSharpArray*)obj;
            array->ItemsType = itemType;
            array->Size = size;
            array->Length = length;

            return obj;
        }
        /// <summary>
        /// Create new instance of string
        /// </summary>
        /// <param name="str">String to create as D# string</param>
        /// <returns>New instance of D# string</returns>
        public DSharpObject* CreateString(string str)
        {
            fixed (char* chars = str)
            {
                UnmanagedArray<char> array = new(chars, str.Length);
                return CreateString(array);
            }
        }
        /// <summary>
        /// Create new instance of string
        /// </summary>
        /// <param name="chars">String characters</param>
        /// <param name="length">String length</param>
        /// <returns>New instance of D# string</returns>
        public DSharpObject* CreateString(char* chars, int length)
        {
            UnmanagedArray<char> array = new(chars, length);
            return CreateString(array);
        }
        /// <summary>
        /// Create new instance of string
        /// </summary>
        /// <param name="str">String characters</param>
        /// <returns>New instance of D# string</returns>
        public DSharpObject* CreateString(UnmanagedArray<char> str)
        {
            var obj = CreateString(str.Length);
            char* chars = DSharpObject.GetData<char>(obj);

            for (int i = 0; i < str.Length; i++)
            {
                chars[i] = str[i];
            }

            return obj;
        }
        /// <summary>
        /// Create new instance of empty string
        /// </summary>
        /// <param name="length">String length</param>
        /// <returns>New instance of D# string</returns>
        public DSharpObject* CreateString(int length)
        {
            var runtimeStringType = _runtimeInformationProvider.String;
            var runtimeCharType = _runtimeInformationProvider.Char;

            var size = runtimeStringType->Size + runtimeCharType->Size * length;
            var obj = Create(runtimeStringType, size, true);
            var array = (DSharpArray*)obj;
            array->ItemsType = runtimeCharType;
            array->Size = size;
            array->Length = length;
            obj->Attributes |= DSharpObjectAttributes.String;

            return obj;
        }

        /// <summary>
        /// Box structure into heap
        /// </summary>
        /// <param name="structure">Structure that need to boxed</param>
        /// <returns>Pointer to boxed structure</returns>
        public DSharpObject* Box(DSharpObject* structure)
        {
            if (structure->IsReferenceObject)
            {
                return structure;
            }

            var obj = _memoryManager.Allocate<DSharpObject>(DSharpMemoryBlockType.Object, DSharpObject.GetSize(structure));

            DSharpObject.Copy(structure, obj);

            obj->Placement = DSharpObjectPlacement.Heap;

            return obj;
        }
        /// <summary>
        /// Unbox D# object into buffer.
        /// It unboxes only structures
        /// </summary>
        /// <param name="instance">D# object instance to unboxing</param>
        /// <param name="buffer">Buffer for writing unboxed object</param>
        /// <returns>Is object unboxed</returns>
        public bool Unbox(DSharpObject* instance, UnmanagedArray<byte> buffer)
        {
            if (instance == null || buffer.Length == 0 ||
                !instance->Type->IsValueType)
            {
                return false;
            }

            var totalSize = DSharpObject.GetTotalSize(instance);

            if (totalSize > buffer.Length)
            {
                throw new ArgumentException($"Unable to unbox \"{instance->ToString()}\": buffer too small", nameof(buffer));
            }

            DSharpObject.Copy(instance, (DSharpObject*)buffer.GetItemReference(0));

            return true;
        }

        /// <summary>
        /// Write number to structure with number type
        /// </summary>
        /// <param name="obj">Structure with number type</param>
        /// <param name="value">Number to write. It will be automatically converted</param>
        /// <exception cref="InvalidOperationException">Unable to write number</exception>
        public void WriteNumber(DSharpObject* obj, decimal value)
        {
            var type = obj->Type;

            void Write<T>(T value) where T : unmanaged
            {
                *(T*)DSharpObject.GetData(obj) = value;
            }

            if (type == _runtimeInformationProvider.Byte)
            {
                value = Math.Clamp(value, byte.MinValue, byte.MaxValue);
                Write(decimal.ToByte(value));
            }
            else if (type == _runtimeInformationProvider.SByte)
            {
                value = Math.Clamp(value, sbyte.MinValue, sbyte.MaxValue);
                Write(decimal.ToSByte(value));
            }
            else if (type == _runtimeInformationProvider.Char)
            {
                value = Math.Clamp(value, char.MinValue, char.MaxValue);
                Write((char)decimal.ToInt16(value));
            }
            else if (type == _runtimeInformationProvider.Int16)
            {
                value = Math.Clamp(value, short.MinValue, short.MaxValue);
                Write(decimal.ToInt16(value));
            }
            else if (type == _runtimeInformationProvider.UInt16)
            {
                value = Math.Clamp(value, ushort.MinValue, ushort.MaxValue);
                Write(decimal.ToUInt16(value));
            }
            else if (type == _runtimeInformationProvider.Int32)
            {
                value = Math.Clamp(value, int.MinValue, int.MaxValue);
                Write(decimal.ToInt32(value));
            }
            else if (type == _runtimeInformationProvider.UInt32)
            {
                value = Math.Clamp(value, uint.MinValue, uint.MaxValue);
                Write(decimal.ToUInt32(value));
            }
            else if (type == _runtimeInformationProvider.Int64)
            {
                value = Math.Clamp(value, long.MinValue, long.MaxValue);
                Write(decimal.ToInt64(value));
            }
            else if (type == _runtimeInformationProvider.UInt64)
            {
                value = Math.Clamp(value, ulong.MinValue, ulong.MaxValue);
                Write(decimal.ToUInt64(value));
            }
            else if (type == _runtimeInformationProvider.IntPtr)
            {
                if (sizeof(nint) == sizeof(long))
                {
                    value = Math.Clamp(value, long.MinValue, long.MaxValue);
                    Write((nint)decimal.ToInt64(value));
                }
                else
                {
                    value = Math.Clamp(value, int.MinValue, int.MaxValue);
                    Write((nint)decimal.ToInt32(value));
                }
            }
            else if (type == _runtimeInformationProvider.UIntPtr)
            {
                if (sizeof(nuint) == sizeof(long))
                {
                    value = Math.Clamp(value, ulong.MinValue, ulong.MaxValue);
                    Write((nuint)decimal.ToUInt64(value));
                }
                else
                {
                    value = Math.Clamp(value, uint.MinValue, uint.MaxValue);
                    Write((nuint)decimal.ToUInt32(value));
                }
            }
            else if (type == _runtimeInformationProvider.Single)
            {
                Write(decimal.ToSingle(value));
            }
            else if (type == _runtimeInformationProvider.Double)
            {
                Write(decimal.ToDouble(value));
            }
            else if (type == _runtimeInformationProvider.Decimal)
            {
                Write(value);
            }
            else
            {
                throw new InvalidOperationException($"Unable to write number \"{value}\" to object with type \"{obj->Type->ToString()}\"");
            }
        }

        private DSharpObject* Create(DSharpRuntimeTypeInfo* type, int size, bool isArray = false)
        {
            if (type->IsValueType)
            {
                throw new ArgumentException($"Unable to create instance of \"{type->ToString()}\" because it value type", nameof(type));
            }
            if (isArray)
            {
                size += sizeof(DSharpArray);
            }
            else
            {
                size += sizeof(DSharpObject);
            }

            var obj = (DSharpObject*)_memoryManager.Allocate(DSharpMemoryBlockType.Object, size);
            Setup(obj, DSharpObjectAttributes.StoredInHeap, type, isArray);

            _objects.Add((nint)obj);

            return obj;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Setup(DSharpObject* obj, DSharpObjectAttributes baseAttributes, DSharpRuntimeTypeInfo* type, bool isArray)
        {
            obj->Type = type;
            obj->Attributes = baseAttributes;

            if (isArray)
            {
                obj->Attributes |= DSharpObjectAttributes.Array;
            }
            if (type->Initializer == null)
            {
                obj->Attributes |= DSharpObjectAttributes.Initialized;
            }
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!_memoryManager.IsDisposed)
            {
                foreach (var obj in _objects)
                {
                    _memoryManager.Free(obj);
                }
            }


            _objects.Clear();
        }

        #endregion

        #region Static

        /// <summary>
        /// Create structure with specified type and data in buffer
        /// </summary>
        /// <param name="type">Type of structure for creating</param>
        /// <param name="data">Data for filling structure</param>
        /// <param name="buffer">Buffer that will be filled with structure</param>
        /// <param name="isArray">Is array structure</param>
        /// <returns>Pointer to structure</returns>
        public static DSharpObject* CreateStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> data, UnmanagedArray<byte> buffer, bool isArray)
        {
            if (type->Size > buffer.Length)
            {
                throw new ArgumentException($"Provided buffer should have same size to type, got \"{buffer.Length}\" but required \"{type->Size}\"", nameof(buffer));
            }

            DSharpObject* obj = (DSharpObject*)buffer.GetItemReference(0);
            obj->Attributes = DSharpObjectAttributes.StoredInBuffer;
            obj->Type = type;

            if (isArray)
            {
                obj->Attributes |= DSharpObjectAttributes.Array;
            } 

            int sizeForData = buffer.Length - sizeof(DSharpObject);
            byte* objectDataBuffer = DSharpObject.GetData(obj);

            if (0 > sizeForData)
            {
                return obj;
            }
            if (data.Length == 0 && sizeForData > 0)
            {
                RuntimeExtensions.FillZero(objectDataBuffer, sizeForData);
                return obj;
            }
            if (data.Length > 0)
            {
                var dataBuffer = data.GetItemReference(0);
                Buffer.MemoryCopy(dataBuffer, objectDataBuffer, sizeForData, data.Length);
            }
            else
            {
                RuntimeExtensions.FillZero(objectDataBuffer, sizeForData);
            }

            return obj;
        }
        /// <summary>
        /// Create structure that placed in last value in stack.
        /// It will be created in provided buffer
        /// </summary>
        /// <param name="stack">Stack that contains structure or value type in last value</param>
        /// <param name="buffer">Buffer that will be used for creating structure</param>
        /// <returns>Created structure</returns>
        public static DSharpObject* CreateStructureFromStack(DSharpStack stack, UnmanagedArray<byte> buffer)
        {
            if (stack.Count == 0)
            {
                throw new ArgumentException("Stack is empty");
            }

            var lastValue = stack.Peek();

            if (lastValue.ValueType == DSharpStackValueType.Structure)
            {
                if (lastValue.Size > buffer.Length)
                {
                    throw new ArgumentException($"Buffer size should be equals or greater size then structure size. Requires \"{lastValue.Size}\", got: \"{buffer.Length}\"");
                }

                for (int i = 0; i < lastValue.Size; i++)
                {
                    buffer[i] = lastValue[i];
                }

                return (DSharpObject*)buffer.GetItemReference(0);
            }

            throw new InvalidOperationException($"Unable to create structure: invalid value type \"{lastValue.ValueType}\"");
        }
        /// <summary>
        /// Try to get size for creating structure buffer
        /// </summary>
        /// <param name="stack">Stack that contains structure or value type in last value</param>
        /// <param name="result">Size of structure</param>
        /// <returns>Is size found successfully</returns>
        public static bool TryGetSizeForStructureFromStack(DSharpStack stack, out int result)
        {
            result = -1;

            if (stack.Count == 0)
            {
                return false;
            }

            var lastValue = stack.Peek();

            if (lastValue.ValueType == DSharpStackValueType.Structure)
            {
                result = lastValue.Size;
                return true;
            }

            return false;
        }

        #endregion
    }
}
