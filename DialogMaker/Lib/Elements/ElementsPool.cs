namespace DialogMaker.Lib.Elements
{
    public class ElementsPool<T>(Func<T> fabric) : IDisposable
    {
        public ElementsPool()
            : this(() => Activator.CreateInstance<T>())
        {
        }
        ~ElementsPool()
        {
            Dispose();
        }

        private readonly Queue<T> _freeElements = new();
        private readonly List<T> _usedElements = [];
        private readonly Func<T> _fabric = fabric;

        #region Управление

        public T GetElement()
        {
            if (!_freeElements.TryDequeue(out var result))
            {
                result = _fabric();
            }

            _usedElements.Add(result);

            return result;
        }
        public bool Free(T element)
        {
            if (_usedElements.Remove(element))
            {
                _freeElements.Enqueue(element);
                return true;
            }

            return false;
        }
        public void Clear()
        {
            foreach (var element in _usedElements)
            {
                _freeElements.Enqueue(element);
            }

            _usedElements.Clear();
        }

        public void Dispose()
        {
            _freeElements.Clear();
            _freeElements.EnsureCapacity(4);
            _usedElements.Clear();
            _usedElements.EnsureCapacity(4);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
