using DialogMaker.Core.Scripting.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting
{
    internal class ElementsPool<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Func<T> fabric) 
        : DSharpDisposable
    {
        public ElementsPool() : this(Activator.CreateInstance<T>)
        {
        }

        private readonly List<T> _used = [];
        private readonly Queue<T> _free = [];

        #region Controls

        public T GetElement()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (!_free.TryDequeue(out var item))
            {
                item = fabric();
            }

            _used.Add(item);

            return item;
        }
        public bool Free(T item)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (_used.Remove(item))
            {
                _free.Enqueue(item);
                return true;
            }

            return false;
        }
        public void FreeAll()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            foreach (var item in _used)
            {
                _free.Enqueue(item);
            }

            _used.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            static void Dispose(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    if (item is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }

            Dispose(_used);
            Dispose(_free);

            _used.Clear();
            _free.Clear();
        }

        #endregion
    }
}
