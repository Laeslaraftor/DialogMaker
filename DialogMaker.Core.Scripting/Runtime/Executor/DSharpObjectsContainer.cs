using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
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

        private DSharpObject* Create(DSharpRuntimeTypeInfo* type, int size)
        {
            if (!type->IsValueType)
            {
                throw new ArgumentException($"Unable to create instance of \"{type->ToString()}\" because it value types", nameof(type));
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
