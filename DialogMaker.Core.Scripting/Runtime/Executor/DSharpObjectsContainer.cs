using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

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
        /// <returns>New instance of D# array</returns>
        public DSharpObject* CreateArray(DSharpRuntimeTypeInfo* type, int length)
        {
            var size = ((DSharpRuntimeTypeInfo*)type->GenericParameters[0])->ItemSize * length;
            var obj = Create(type, size);
            obj->Length = length;

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
            if (Assembly.GetType(DSharpBuildInTypes.String) is not IDSharpType stringType)
            {
                throw new InvalidOperationException("Unable to find string type for creating new instance of D# string");
            }

            var runtimeStringType = _runtimeInformationProvider.GetRuntimeInfo(stringType);
            var obj = Create(runtimeStringType, runtimeStringType->Size + sizeof(char) * str.Length);
            obj->Length = str.Length;
            char* chars = (char*)DSharpObject.GetData(obj);

            for (int i = 0; i < str.Length; i++)
            {
                chars[i] = str[i];
            }

            return obj;
        }

        /// <summary>
        /// Box structure into heap
        /// </summary>
        /// <param name="structure">Structure that need to boxed</param>
        /// <returns>Pointer to boxed structure</returns>
        public DSharpObject* Box(DSharpObject* structure)
        {
            if (structure->Placement == DSharpObjectPlacement.Heap)
            {
                return structure;
            }

            var obj = _memoryManager.Allocate<DSharpObject>(DSharpMemoryBlockType.Object, structure->Size);

            DSharpObject.Copy(structure, obj);

            obj->Placement = DSharpObjectPlacement.Heap;

            return obj;
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

        private DSharpObject* Create(DSharpRuntimeTypeInfo* type, int size)
        {
            if (type->IsValueType)
            {
                throw new ArgumentException($"Unable to create instance of \"{type->ToString()}\" because it value type", nameof(type));
            }

            var obj = _memoryManager.Allocate<DSharpObject>(DSharpMemoryBlockType.Object, size);
            obj->Placement = DSharpObjectPlacement.Heap;
            obj->Type = type;
            obj->Size = size;
            obj->Length = 0;

            _objects.Add((nint)obj);

            return obj;
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
        /// <returns>Pointer to structure</returns>
        public static DSharpObject* CreateStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> data, UnmanagedArray<byte> buffer)
        {
            if (type->Size > buffer.Length)
            {
                throw new ArgumentException($"Provided buffer should have same size to type, got \"{buffer.Length}\" but required \"{type->Size}\"", nameof(buffer));
            }

            DSharpObject* obj = (DSharpObject*)buffer.GetItemReference(0);
            obj->Placement = DSharpObjectPlacement.Buffer;
            obj->Type = type;
            obj->Length = 0;
            obj->Size = type->Size;

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

            var dataBuffer = data.GetItemReference(0);

            Buffer.MemoryCopy(dataBuffer, objectDataBuffer, sizeForData, data.Length);

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
