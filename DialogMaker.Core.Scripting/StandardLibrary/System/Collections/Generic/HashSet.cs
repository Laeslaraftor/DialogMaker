namespace System.Collections.Generic;

public class HashSet<T> : IEnumerable<T>
{
    public HashSet() : this(4)
    {
    }
    public HashSet(int capacity)
    {
        capacity = Math.Max(capacity, 4);
        _buckets = new int[capacity];
        _slots = new ItemInfo[capacity];

        Clear(false);
    }

    public int Count { get; private set; }
    public int Capacity => _buckets.Length;
    public T this[int index]
    {
        get
        {
            int count = 0;

            for (int i = 0; i < _buckets.Length; i++)
            {
                int slotIndex = _buckets[i];

                while (slotIndex != -1)
                {
                    var item = _slots[slotIndex];
                    
                    if (count >= index)
                    {
                        return item.Item;
                    }

                    count++;
                    slotIndex = item.Next;
                }
            }

            throw new IndexOutOfRangeException();
        }
    }

    private int[] _buckets;
    private ItemInfo[] _slots;
    private int _firstFreeSlotIndex;

    public bool Add(T item) => Add(item, true);
    public void Clear() => Clear(true);
    public bool Contains(T item)
    {
        int index = GetIndex(item, out int hashCode);
        int slotIndex = _buckets[index];

        while (slotIndex != -1)
        {
            var itemInfo = _slots[slotIndex];

            if (itemInfo.HashCode == hashCode && Equals(itemInfo.Item, item))
            {
                return true;
            }

            slotIndex = itemInfo.Next;
        }

        return false;
    }
    public bool Remove(T item)
    {
        int index = GetIndex(item, out int hashCode);
        int slotIndex = _buckets[index];
        int previousSlotIndex = -1;

        while (slotIndex != -1)
        {
            var itemInfo = _slots[slotIndex];

            if (itemInfo.HashCode == hashCode && Equals(itemInfo.Item, item))
            {
                AddFreeSlot(slotIndex);

                if (previousSlotIndex != -1)
                {
                    var previousSlot = _slots[previousSlotIndex];
                    previousSlot.Next = slotIndex;
                    _slots[previousSlotIndex] = previousSlot;
                }

                Count--;

                return true;
            }

            previousSlotIndex = slotIndex;
            slotIndex = itemInfo.Next;
        }

        return false;
    }
    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    private bool Add(T item, bool countAndResize)
    {
        if (countAndResize)
        {
            ResizeIfNeed();
        }

        int index = GetIndex(item, out int hashCode);
        int slotIndex = _buckets[index];
        ItemInfo slot;
       
        if (slotIndex == -1)
        {
            slot = ItemInfo.Empty;
            slotIndex = GetFreeSlot();
        }
        else
        {
            slot = _slots[slotIndex];
        }

        if (slot.IsEmpty)
        {
            slot.HashCode = hashCode;
            slot.Item = item;
            slot.IsEmpty = false;
            _slots[slotIndex] = slot;

            if (countAndResize)
            {
                Count++;
            }

            return true;
        }

        while (slotIndex != -1)
        {
            if (hashCode == slot.HashCode && Equals(item, slot.Item))
            {
                return false;
            }

            slotIndex = slot.Next;

            if (slotIndex != -1)
            {
                slot = _slots[slotIndex];
            }
        }

        var nextSlotIndex = GetFreeSlot();
        slot.Next = nextSlotIndex;
        _slots[slotIndex] = slot;
        _slots[nextSlotIndex] = new()
        {
            HashCode = hashCode,
            Item = item,
            Next = -1,
            IsEmpty = false
        };

        if (countAndResize)
        {
            Count++;
        }

        return true;
    }
    private void Clear(bool resetCount)
    {
        if (resetCount)
        {
            Count = 0;
        }

        _firstFreeSlotIndex = 0;
        Array<int>.Fill(_buckets, -1);

        for (int i = 0; i < _slots.Length; i++)
        {
            var item = ItemInfo.Empty;
            item.Next = i + 1;
            _slots[i] = item;
        }
    }
    private void ResizeIfNeed()
    {
        int capacity = _buckets.Length; 

        if (Count + 2 < capacity)
        {
            return;
        }

        int newCapacity = capacity * 2;
        int[] newBuckets = new int[newCapacity];
        ItemInfo[] newSlots = new ItemInfo[newCapacity];

        var oldBuckets = _buckets;
        var oldSlots = _slots;
        _buckets = newBuckets;
        _slots = newSlots;

        Clear(false);

        for (int i = 0; i < capacity; i++)
        {
            int index = oldBuckets[i];

            while (index != -1)
            {
                var itemInfo = oldSlots[index];
                Add(itemInfo.Item, false);
                index = itemInfo.Next;
            }
        }
    }
    private int GetFreeSlot()
    {
        int index = _firstFreeSlotIndex;

        if (index == -1)
        {
            throw new InvalidOperationException("Unable to get free slot");
        }

        _firstFreeSlotIndex = _slots[index].Next;

        return index;
    }
    private void AddFreeSlot(int index)
    {
        var slot = ItemInfo.Empty;
        slot.Next = _firstFreeSlotIndex;
        _slots[index] = slot;
        _firstFreeSlotIndex = index;
    }
    private int GetIndex(T item, out int hashCode)
    {
        hashCode = item == null ? 0 : item.GetHashCode();
        return hashCode % (_buckets.Length - 1);
    }

    private struct ItemInfo
    {
        public int HashCode;
        public int Next;
        public T Item;
        public bool IsEmpty;

        public static readonly ItemInfo Empty = new()
        {
            HashCode = -1,
            Next = -1,
            IsEmpty = true
        };
    }
    private class Enumerator : IEnumerator<T>
    {
        public Enumerator(HashSet<T> hashSet)
        {
            _hashSet = hashSet;
        }

        public T Current { get; private set; }

        private readonly HashSet<T> _hashSet;
        private int _lastBucketIndex = -1;
        private int _nextSlotIndex = -1;

        public bool MoveNext()
        {
            if (_nextSlotIndex != -1)
            {
                var slot = _hashSet._slots[_nextSlotIndex];
                Current = slot.Item;
                _nextSlotIndex = slot.Next;

                return true;
            }

            while (_hashSet.Capacity > _lastBucketIndex)
            {
                _lastBucketIndex++;
                var slotIndex = _hashSet._buckets[_lastBucketIndex];

                if (slotIndex == -1)
                {
                    continue;
                }

                var slot = _hashSet._slots[slotIndex];
                Current = slot.Item;
                _nextSlotIndex = slot.Next;

                return true;
            }

            return false;
        }
        public void Reset()
        {
            Current = null;
            _nextSlotIndex = -1;
            _lastBucketIndex = -1;
        }
    }
}