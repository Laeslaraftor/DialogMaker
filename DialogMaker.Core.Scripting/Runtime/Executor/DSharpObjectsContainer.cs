using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Objects instances container
    /// </summary>
    public unsafe class DSharpObjectsContainer : Disposable
    {
        private readonly List<nint> _objects = [];

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
