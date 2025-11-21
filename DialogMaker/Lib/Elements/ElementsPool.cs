namespace DialogMaker.Lib.Elements
{
    public class ElementsPool<T> : IDisposable where T : new()
    {
        private readonly Queue<T> _freeElements = new();
        private readonly List<T> _usedElements = [];

        #region Управление

        public T GetElement()
        {
            if (!_freeElements.TryDequeue(out var result))
            {
                result = new();
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
        public void FreeAll()
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
        }

        #endregion
    }
}
