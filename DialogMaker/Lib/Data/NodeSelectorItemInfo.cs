using DialogMaker.Core;
using System.Collections.Specialized;

namespace DialogMaker.Lib.Data
{
    public class NodeSelectorItemInfo : Disposable
    {
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
        public IList<NodeSelectorItemInfo>? Children
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Children));

                    if (field is INotifyCollectionChanged oldObservable)
                    {
                        oldObservable.CollectionChanged -= OnObservableCollectionChanged;
                    }

                    field = value;

                    if (value is INotifyCollectionChanged newObservable)
                    {
                        newObservable.CollectionChanged += OnObservableCollectionChanged;
                    }

                    if (value == null)
                    {
                        IsEmpty = true;
                    }
                    else
                    {
                        IsEmpty = value.Count == 0;
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

        public NodeSelectorItemInfo Copy()
        {
            NodeSelectorItemInfo result = new()
            {
                IsEnabled = IsEnabled,
                IsMinimized = IsMinimized,
                IsContainer = IsContainer,
                Name = Name,
                Value = Value,
            };
            var children = Children;

            if (children != null)
            {
                var childrenCopy = (IList<NodeSelectorItemInfo>?)Activator.CreateInstance(children.GetType());

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

        #endregion
    }
}
