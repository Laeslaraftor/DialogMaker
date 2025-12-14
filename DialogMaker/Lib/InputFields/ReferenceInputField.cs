using DialogMaker.Core;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public class ReferenceInputField : InputField
    {
        public ReferenceInputField()
        {
            _view = new();
            _view.ItemChanged += OnViewItemChanged;
        }

        public override string Placeholder
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Placeholder));

                    field = value;

                    if (_view.Placeholder != value)
                    {
                        _view.Placeholder = value;
                    }

                    InvokePropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Value));
                    field = value;

                    if (_view.Item?.Equals(value) != true)
                    {
                        _view.Item = EditorExtensions.ToEditorItem(value);
                    }

                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        public DialogResourceType? ResourceType
        {
            get => field;
            set
            {
                if (_view.RequestedResourceType != value)
                {
                    InvokePropertyChanging(nameof(ResourceType));

                    field = value;

                    if (_view.RequestedResourceType != value)
                    {
                        _view.RequestedResourceType = value;
                    }

                    InvokePropertyChanged(nameof(ResourceType));
                }
            }
        }

        public override FrameworkElement View => _view;

        protected readonly ReferenceView _view;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            _view.ItemChanged -= OnViewItemChanged;
        }

        #endregion

        #region События

        private void OnViewItemChanged(object? sender, ValueChangedEventArgs<ProjectResourceItem?> e)
        {
            Value = e.NewValue;
        }

        #endregion
    }
}
