namespace DialogMaker.Lib.Elements
{
    public class ElementsPool(Func<object> fabric) : Disposable
    {
        public ElementsPool(Type objectType)
            : this(() => Activator.CreateInstance(objectType) ?? throw new InvalidOperationException("Не удалось создать объект"))
        {
        }

        private readonly Queue<object> _freeElements = new();
        private readonly List<object> _usedElements = [];
        private readonly Func<object> _fabric = fabric;

        #region Управление

        public object GetElement()
        {
            if (!_freeElements.TryDequeue(out var result))
            {
                result = _fabric();
            }

            _usedElements.Add(result);

            return result;
        }
        public bool Free(object element)
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _freeElements.Clear();
            _freeElements.EnsureCapacity(4);
            _usedElements.Clear();
            _usedElements.EnsureCapacity(4);
        }

        #endregion
    }
}
