using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Objects instances container
    /// </summary>
    public unsafe class DSharpObjectsContainer(IDSharpAssembly assembly, DSharpRuntimeInformationProvider runtimeInformationProvider) : Disposable
    {
        /// <summary>
        /// D# assembly
        /// </summary>
        public IDSharpAssembly Assembly { get; } = assembly;

        private readonly List<nint> _objects = [];
        private readonly DSharpRuntimeInformationProvider _runtimeInformationProvider = runtimeInformationProvider;

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
            char* chars = (char*)obj + sizeof(DSharpObject);

            for (int i = 0; i < str.Length; i++)
            {
                chars[i] = str[i];
            }

            return obj;
        }

        /// <summary>
        /// Create structure with specified type and data in buffer
        /// </summary>
        /// <param name="type">Type of structure for creating</param>
        /// <param name="data">Data for filling structure</param>
        /// <param name="buffer">Buffer that will be filled with structure</param>
        /// <returns>Pointer to structure</returns>
        public DSharpObject* CreateStructure(DSharpRuntimeTypeInfo* type, UnmanagedArray<byte> data, UnmanagedArray<byte> buffer)
        {
            if (type->Size > buffer.Length)
            {
                throw new ArgumentException($"Provided buffer should have same size to type, got \"{buffer.Length}\" but required \"{type->Size}\"", nameof(buffer));
            }

            DSharpObject* obj = (DSharpObject*)buffer.GetItemReference(0);
            obj->Type = type;
            obj->Length = 0;
            obj->Size = type->Size;

            int sizeForData = buffer.Length - sizeof(DSharpObject);
            byte* objectDataBuffer = buffer.GetItemReference(sizeof(DSharpObject));

            for (int i = 0; i < Math.Min(sizeForData, data.Length); i++)
            {
                objectDataBuffer[i] = data[i];
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
        public DSharpObject* CreateStructureFromStack(DSharpStack stack, UnmanagedArray<byte> buffer)
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
            else if (lastValue.IsNumber || lastValue.ValueType == DSharpStackValueType.Bool)
            {
                if (!DSharpStack.BuildInValueTypes.TryGetValue(lastValue.ValueType, out var typeInfo))
                {
                    throw new InvalidOperationException($"Unable to create structure: failed to get information for build-in type \"{lastValue.ValueType}\"");
                }
                if (Assembly.GetType(typeInfo) is not IDSharpType type)
                {
                    throw new InvalidOperationException($"Unable to create structure: failed to get build-in type \"{lastValue.ValueType}\" from \"{Assembly}\"");
                }

                var runtimeType = _runtimeInformationProvider.GetRuntimeInfo(type);
                UnmanagedArray<byte> data = new(lastValue.StackPointer, lastValue.Size);

                return CreateStructure(runtimeType, data, buffer);
            }

            throw new InvalidOperationException($"Unable to create structure: invalid value type \"{lastValue.ValueType}\"");
        }
        /// <summary>
        /// Try to get size for creating structure buffer
        /// </summary>
        /// <param name="stack">Stack that contains structure or value type in last value</param>
        /// <param name="result">Size of structure</param>
        /// <returns>Is size found successfully</returns>
        public bool TryGetSizeForStructureFromStack(DSharpStack stack, out int result)
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
            else if (lastValue.IsNumber || lastValue.ValueType == DSharpStackValueType.Bool)
            {
                if (!DSharpStack.BuildInValueTypes.TryGetValue(lastValue.ValueType, out var typeInfo))
                {
                    throw new InvalidOperationException($"Unable to get structure size: failed to get information for build-in type \"{lastValue.ValueType}\"");
                }
                if (Assembly.GetType(typeInfo) is not IDSharpType type)
                {
                    throw new InvalidOperationException($"Unable to get structure size: failed to get build-in type \"{lastValue.ValueType}\" from \"{Assembly}\"");
                }

                var runtimeType = _runtimeInformationProvider.GetRuntimeInfo(type);
                result = runtimeType->Size + sizeof(DSharpObject);

                return true;
            }

            return false;
        }

        private DSharpObject* Create(DSharpRuntimeTypeInfo* type, int size)
        {
            if (type->IsValueType)
            {
                throw new ArgumentException($"Unable to create instance of \"{type->ToString()}\" because it value type", nameof(type));
            }

            DSharpObject* obj = (DSharpObject*)Marshal.AllocHGlobal(sizeof(DSharpObject) + size);
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

            foreach (var obj in _objects)
            {
                Marshal.FreeHGlobal(obj);
            }

            _objects.Clear();
        }

        #endregion

    }
}
