using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using System.Collections.Specialized;

namespace DialogMaker.Lib.Data
{
    public class NodeSelectorItemInfo : Disposable
    {
        public event EventHandler<ItemEventArgs<NodeSelectorItemInfo>>? BringToViewRequested;

        public bool IsEnabled
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsEnabled));
                    field = value;
                    InvokePropertyChanged(nameof(IsEnabled));
                }
            }
        }
        public bool IsEmpty
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsEmpty));
                    field = value;
                    InvokePropertyChanged(nameof(IsEmpty));
                }
            }
        }
        public bool IsMinimized
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsMinimized));
                    field = value;
                    InvokePropertyChanged(nameof(IsMinimized));
                }
            }
        }
        public bool IsContainer
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsContainer));
                    field = value;
                    InvokePropertyChanged(nameof(IsContainer));
                }
            }
        }
        public bool IsSelected
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsSelected));
                    field = value;
                    InvokePropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string? Name
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Name));
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }
        public EditableCollection<NodeSelectorItemInfo>? Children
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Children));

                    field?.ItemChanged -= OnNodeSelectorItemInfoItemChanged;
                    field = value;
                    value?.ItemChanged += OnNodeSelectorItemInfoItemChanged;

                    if (value == null)
                    {
                        IsEmpty = true;
                    }
                    else
                    {
                        IsEmpty = value.Count == 0;

                        foreach (var item in value)
                        {
                            item.BringToViewRequested += OnItemBringToViewRequested;
                        }
                    }

                    InvokePropertyChanged(nameof(Children));
                }
            }
        }
        public object? Value

        {
            get => field;
            set
            {
                if (!Equals(field, value))
                {
                    InvokePropertyChanging(nameof(Value));
                    field = value;
                    InvokePropertyChanged(nameof(Value));
                }
            }
        }

        #region Управление

        public void RequestBringToView()
        {
            BringToViewRequested?.Invoke(this, new(this));
        }

        public NodeSelectorItemInfo Copy()
        {
            NodeSelectorItemInfo result = new()
            {
                IsEnabled = IsEnabled,
                IsMinimized = IsMinimized,
                IsContainer = IsContainer,
                IsSelected = IsSelected,
                Name = Name,
                Value = Value,
            };
            var children = Children;

            if (children != null)
            {
                var childrenCopy = (EditableCollection<NodeSelectorItemInfo>?)Activator.CreateInstance(children.GetType());

                if (childrenCopy != null)
                {
                    foreach (var child in children)
                    {
                        childrenCopy.Add(child.Copy());
                    }

                    result.Children = childrenCopy;
                }
            }

            return result;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            var children = Children;

            if (children != null)
            {
                foreach (var child in children)
                {
                    child.BringToViewRequested -= OnItemBringToViewRequested;
                }
            }

            Children = null;
        }

        #endregion

        #region События

        private void OnObservableCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is IList<NodeSelectorItemInfo> list)
            {
                IsEmpty = list.Count == 0;
            }
        }
        private void OnNodeSelectorItemInfoItemChanged(object? sender, CollectionItemEventArgs<NodeSelectorItemInfo> e)
        {
            if (sender is not EditableCollection<NodeSelectorItemInfo> list ||
                e.Action == CollectionItemAction.Move)
            {
                return;
            }
            if (e.Action == CollectionItemAction.Add)
            {
                if (e.Item.IsContainer)
                {
                    return;
                }

                e.Item.BringToViewRequested += OnItemBringToViewRequested;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.BringToViewRequested -= OnItemBringToViewRequested;
            }

            IsEmpty = list.Count == 0;
        }

        private void OnItemBringToViewRequested(object? sender, ItemEventArgs<NodeSelectorItemInfo> e)
        {
            BringToViewRequested?.Invoke(this, e);
        }

        #endregion
    }
}
