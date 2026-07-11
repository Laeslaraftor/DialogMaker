namespace System.Collections.Generic;

public class List<T> : IList<T>
{
    public List() : this(4)
    {
    }
    public List(int capacity)
    {
        capacity = Math.Max(capacity, 4);
        _items = new T[capacity];
    }

    public int Count { get; private set; }
    public int Capacity => _items.Length;
    public T this[int index]
    {
        get
        {
            if (0 > index || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            return _items[index];
        }
        set
        {
            if (0 > index || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            _items[index] = value;
        }
    }

    private T[] _items;

    public int IndexOf(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (item == _items[i])
            {
                return i;
            }
        }

        return -1;
    }
    public bool Contains(T item) => IndexOf(item) != -1;
    public void Add(T item)
    {
        ExpandIfNeed();
        _items[Count] = item;
        Count++;
    }
    public void Insert(T item, int index)
    {
        if (0 > index || index > Count)
        {
            throw new IndexOutOfRangeException();
        }
        if (index == Count)
        {
            Add(item);
            return;
        }

        ExpandIfNeed();

        for (int i = Count - 1; i >= index; i++)
        {
            _items[i + 1] = _items[i];
        }

        _items[index] = item;
        Count++;
    }
    public bool Remove(T item)
    {
        bool isFound = false;

        for (int i = 0; i < Count; i++)
        {
            if (_items[i] == item)
            {
                isFound = true;
                continue;
            }
            if (!isFound)
            {
                continue;
            }

            _items[i - 1] = _items[i];
        }

        if (isFound)
        {
            Count--;
        }

        return isFound;
    }
    public void Clear()
    {
        for (int i = 0; i < Count; i++)
        {
            _items[i] = null;
        }

        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    private void ExpandIfNeed()
    {
        if (Count + 1 > Capacity)
        {
            Expand();
        }
    }
    private void Expand()
    {
        T[] newItems = new T[_items.Length * 2];
        Array<T>.Copy(_items, newItems);
        _items = newItems;
    }    

    private class Enumerator : IEnumerator<T>
    {
        public Enumerator(List<T> list)
        {
            _list = list;
        }

        public T Current { get; private set; }
        
        private readonly List<T> _list;
        private int _currentIndex = -1;

        public bool MoveNext()
        {
            if (_list.Count == 0 || _currentIndex + 1 >= _list.Count)
            {
                return false;
            }

            _currentIndex++;
            Current = _list[_currentIndex];

            return true;
        }
        public void Reset()
        {
            Current = null;
            _currentIndex = -1;
        }
    }
}