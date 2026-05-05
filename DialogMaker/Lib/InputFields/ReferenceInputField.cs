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
                    OnPropertyChanging(nameof(Placeholder));

                    field = value;

                    if (_view.Placeholder != value)
                    {
                        _view.Placeholder = value;
                    }

                    OnPropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => field;
            set
            {
                if (field?.Equals(value) != true)
                {
                    OnPropertyChanging(nameof(Value));
                    field = value;

                    if (value?.Equals(_view.Item) != true)
                    {
                        _view.Item = EditorExtensions.ToEditorItem(value);
                    }

                    OnPropertyChanged(nameof(Value));
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
                    OnPropertyChanging(nameof(ResourceType));

                    field = value;

                    if (_view.RequestedResourceType != value)
                    {
                        _view.RequestedResourceType = value;
                    }

                    OnPropertyChanged(nameof(ResourceType));
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
