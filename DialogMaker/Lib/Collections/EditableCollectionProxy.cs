using System.Collections;
using System.ComponentModel;
using Acly;

namespace DialogMaker.Lib
{
    public class EditableCollectionProxy<T>(EditableCollection<T> list) : IList<T>, INotifyPropertyChanged, IEditableCollectionView, IEditableCollectionViewAddNewItem
    {
        bool IEditableCollectionViewAddNewItem.CanAddNewItem => true;
        bool IEditableCollectionView.CanAddNew => _list.CanAddNew;
        bool IEditableCollectionView.CanCancelEdit => _list.CanCancelEdit;
        bool IEditableCollectionView.CanRemove => _list.CanRemove;
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        object IEditableCollectionView.CurrentAddItem => _list.CurrentAddItem;
        object IEditableCollectionView.CurrentEditItem => _list.CurrentEditItem;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        bool IEditableCollectionView.IsAddingNew => _list.IsAddingNew;
        bool IEditableCollectionView.IsEditingItem => _list.IsEditingItem;
        NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
        {
            get => (NewItemPlaceholderPosition)_list.NewItemPosition;
            set => _list.NewItemPosition = (NewItemPosition)value;
        }
        public int Count => _list.Count;
        public bool IsReadOnly => _list.IsReadOnly;

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        private readonly EditableCollection<T> _list = list;

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Операторы

        public static implicit operator EditableCollectionProxy<T>(EditableCollection<T> list)
        {
            return new(list);
        }

        #endregion

        #region Управление

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }
        public void Add(T item)
        {
            _list.Add(item);
        }


        public bool Remove(T item)
        {
            return ((IList<T>)_list).Remove(item);
        }
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
        public void Clear()
        {
            _list.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        #endregion

        #region Редактирование

        object IEditableCollectionViewAddNewItem.AddNewItem(object newItem)
        {
            if (newItem is T typedItem)
            {
                Add(typedItem);
            }

            return newItem;
        }
        object IEditableCollectionView.AddNew()
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return _list.AddNew();
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }
        void IEditableCollectionView.CancelEdit()
        {
            _list.CancelEdit();
        }
        void IEditableCollectionView.CancelNew()
        {
            _list.CancelNew();
        }
        void IEditableCollectionView.CommitEdit()
        {
            _list.CommitEdit();
        }
        void IEditableCollectionView.CommitNew()
        {
            _list.CommitNew();
        }
        void IEditableCollectionView.EditItem(object item)
        {
            _list.EditItem(item);
        }
        void IEditableCollectionView.Remove(object item)
        {
            ((IEditableCollectionView)_list).Remove(item);
        }

        #endregion

        #region Перечисление

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _list[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
