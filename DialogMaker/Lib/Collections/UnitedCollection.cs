using Acly;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DialogMaker.Lib
{
    public class UnitedCollection<TList, T> : IReadOnlyList<T>, INotifyPropertyChanged, INotifyCollectionChanged, IDisposable
        where TList : IReadOnlyList<T>, INotifyCollectionChanged
    {
        public UnitedCollection(IEnumerable<TList> Collections, ICollectionItemsFilter? Filter = null)
            : this(Collections.ToList(), Filter)
        {

        }
        public UnitedCollection(params TList[] Collections)
            : this(Collections.ToList(), null)
        {

        }
        public UnitedCollection(ICollectionItemsFilter? Filter, params TList[] Collections)
            : this(Collections.ToList(), Filter)
        {

        }
        public UnitedCollection(IList<TList> Collections, ICollectionItemsFilter? Filter = null)
        {
            this.Filter = Filter;
            this.Collections = new ReadOnlyCollection<TList>(Collections);

            InitializeCollections();

            if (Filter != null)
            {
                Filter.FilterChanged += OnFilterChanged;
            }
        }
        ~UnitedCollection()
        {
            Dispose(false);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public T this[int Index] => _CombinedItems[Index];
        public int Count => _CombinedItems.Count;
        public ReadOnlyCollection<TList> Collections { get; }
        public ICollectionItemsFilter? Filter { get; }

        private readonly List<T> _CombinedItems = new();
        private readonly Dictionary<TList, List<T>> _FilteredItems = new();

        #region Управление

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool IsDisposing)
        {
            if (Filter != null)
            {
                Filter.FilterChanged -= OnFilterChanged;
            }

            foreach (var Collection in Collections)
            {
                Collection.CollectionChanged -= OnSourceCollectionChanged;
            }

            _FilteredItems.Clear();
            _CombinedItems.Clear();
        }

        private void InitializeCollections()
        {
            foreach (var Collection in Collections)
            {
                Collection.CollectionChanged -= OnSourceCollectionChanged;
                Collection.CollectionChanged += OnSourceCollectionChanged;
                ProcessCollectionItems(Collection);
            }

            UpdateCombinedItems();
        }
        private void ProcessCollectionItems(TList Collection)
        {
            if (!_FilteredItems.TryGetValue(Collection, out var Filtered))
            {
                Filtered = new();
                _FilteredItems.Add(Collection, Filtered);
            }

            Filtered.Clear();

            foreach (var Item in Collection)
            {
                if (Filter == null || Filter.Check(Collection, Item!))
                {
                    Filtered.Add(Item);
                }
            }
        }
        private void UpdateCombinedItems()
        {
            _CombinedItems.Clear();

            foreach (var Filtered in _FilteredItems.Values)
            {
                _CombinedItems.AddRange(Filtered);
            }
        }
        private int GetCombinedIndexForCollectionItem(TList Collection, int CollectionIndex)
        {
            int CombinedIndex = 0;

            foreach (var List in Collections)
            {
                if (List.Equals(Collection))
                {
                    return CombinedIndex + CollectionIndex;
                }

                if (_FilteredItems.TryGetValue(List, out var Filtered))
                {
                    CombinedIndex += Filtered.Count;
                }
            }

            return -1;
        }

        #endregion

        #region Перечисление

        public IEnumerator<T> GetEnumerator() => _CombinedItems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region События

        private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is not TList Collection ||
                !_FilteredItems.TryGetValue(Collection, out var FilteredItems))
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        List<T> NewFilteredItems = new();

                        foreach (T NewItem in e.NewItems)
                        {
                            if (Filter == null || Filter.Check(Collection, NewItem!))
                            {
                                NewFilteredItems.Add(NewItem);
                            }
                        }

                        if (NewFilteredItems.Count > 0)
                        {
                            int InsertIndex;

                            if (e.NewStartingIndex < FilteredItems.Count)
                            {
                                InsertIndex = e.NewStartingIndex;
                            }
                            else
                            {
                                InsertIndex = FilteredItems.Count;
                            }

                            FilteredItems.InsertRange(InsertIndex, NewFilteredItems);
                            UpdateCombinedItems();

                            var CombinedIndex = GetCombinedIndexForCollectionItem(Collection, InsertIndex);

                            if (CombinedIndex >= 0)
                            {
                                InvokeCollectionChanged(NotifyCollectionChangedAction.Add,
                                    NewFilteredItems, CombinedIndex);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        List<T> RemovedItems = new();
                        int FirstItemIndex = -1;

                        foreach (T oldItem in e.OldItems)
                        {
                            var Index = FilteredItems.IndexOf(oldItem);

                            if (Index >= 0)
                            {
                                FirstItemIndex = Index;
                                RemovedItems.Add(FilteredItems[Index]);
                                FilteredItems.RemoveAt(Index);
                            }
                        }

                        if (RemovedItems.Count > 0)
                        {
                            UpdateCombinedItems();
                            try
                            {
                                InvokeCollectionChanged(NotifyCollectionChangedAction.Remove, RemovedItems, FirstItemIndex);
                            }
                            catch
                            {
                                try
                                {
                                    InvokeCollectionChanged(NotifyCollectionChangedAction.Reset);
                                }
                                catch (Exception error)
                                {
                                    error.Alert();
                                }
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null && e.OldItems != null)
                    {
                        var oldIndex = FilteredItems.IndexOf((T)e.OldItems[0]!);

                        if (oldIndex >= 0)
                        {
                            var OldItem = FilteredItems[oldIndex];
                            FilteredItems.RemoveAt(oldIndex);

                            var NewItem = (T)e.NewItems[0]!;

                            if (Filter == null || Filter.Check(Collection, NewItem!))
                            {
                                FilteredItems.Insert(oldIndex, NewItem);
                                UpdateCombinedItems();

                                var combinedIndex = GetCombinedIndexForCollectionItem(Collection, oldIndex);

                                if (combinedIndex >= 0)
                                {
                                    InvokeCollectionChanged(NotifyCollectionChangedAction.Replace,
                                        NewItem: NewItem, OldItem: OldItem, Index: combinedIndex);
                                }
                            }
                            else
                            {
                                UpdateCombinedItems();
                                InvokeCollectionChanged(NotifyCollectionChangedAction.Remove,
                                    new[] { OldItem });
                            }
                        }
                        else
                        {
                            var NewItem = (T)e.NewItems[0]!;

                            if (Filter == null || Filter.Check(Collection, NewItem!))
                            {
                                var insertIndex = e.NewStartingIndex < FilteredItems.Count
                                    ? e.NewStartingIndex
                                    : FilteredItems.Count;

                                FilteredItems.Insert(insertIndex, NewItem);
                                UpdateCombinedItems();

                                var combinedIndex = GetCombinedIndexForCollectionItem(Collection, insertIndex);

                                if (combinedIndex >= 0)
                                {
                                    InvokeCollectionChanged(NotifyCollectionChangedAction.Add,
                                        new[] { NewItem }, combinedIndex);
                                }
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    UpdateCombinedItems();
                    InvokeCollectionChanged(NotifyCollectionChangedAction.Reset);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ProcessCollectionItems(Collection);
                    UpdateCombinedItems();
                    InvokeCollectionChanged(NotifyCollectionChangedAction.Reset);
                    break;
            }
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            foreach (var Collection in Collections)
            {
                ProcessCollectionItems(Collection);
            }

            UpdateCombinedItems();
            InvokeCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        protected virtual void InvokeCollectionChanged(NotifyCollectionChangedAction Action, IList? ChangedItems = null,
            int Index = -1, object? OldItem = null, object? NewItem = null)
        {
            if (ChangedItems != null)
            {
                NotifyCollectionChangedEventArgs Args;

                if (Action == NotifyCollectionChangedAction.Add ||
                    Action == NotifyCollectionChangedAction.Remove)
                {
                    Args = new(Action, ChangedItems, Index);
                }
                else
                {
                    Args = new(Action, ChangedItems);
                }

                CollectionChanged?.Invoke(this, Args);
            }
            else if (OldItem != null && NewItem != null && Index >= 0)
            {
                CollectionChanged?.Invoke(this, new(Action, NewItem, OldItem, Index));
            }
            else
            {
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
            }

            InvokePropertyChanged(nameof(Count));
        }

        protected virtual void InvokePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        #endregion
    }
}
