using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public unsafe class DSharpObjectsContainer : Disposable
    {
        private readonly List<nint> _objects = [];

        #region Controls

        public DSharpObject* Create(DSharpRuntimeTypeInfo* type) => Create(type, type->Size);
        public DSharpObject* CreateArray(DSharpRuntimeTypeInfo* type, int length)
        {
            var size = ((DSharpRuntimeTypeInfo*)type->GenericParameters[0])->ItemSize * length;
            var obj = Create(type, size);
            obj->Size = length;

            return obj;
        }

        private DSharpObject* Create(DSharpRuntimeTypeInfo* type, int size)
        {
            if (type->ObjectType != DSharpObjectType.Class &&
                type->ObjectType != DSharpObjectType.Struct)
            {
                throw new ArgumentException($"Unable to create instance of \"{type->ToString()}\"", nameof(type));
            }

            DSharpObject* obj = (DSharpObject*)Marshal.AllocHGlobal(sizeof(DSharpObject) + size);
            obj->Type = type;
            obj->Size = size;

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
