using DialogMaker.Core;
using DialogMaker.Lib.Elements;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public class ObjectInputField : InputField
    {
        public ObjectInputField()
        {
            _view.ValueChanged += OnViewValueChanged;
        }

        public override string Placeholder
        {
            get => _view.Placeholder;
            set
            {
                if (_view.Placeholder != value)
                {
                    OnPropertyChanging(nameof(Placeholder));
                    _view.Placeholder = value;
                    OnPropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => _view.Value;
            set
            {
                if (!Equals(_view.Value, value))
                {
                    OnPropertyChanging(nameof(Value));
                    _view.Value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
        public AllowedObjectValues AllowedValues
        {
            get => _view.AllowedValues;
            set
            {
                if (_view.AllowedValues != value)
                {
                    OnPropertyChanging(nameof(AllowedValues));
                    _view.AllowedValues = value;
                    OnPropertyChanged(nameof(AllowedValues));
                }
            }
        }
        public DialogResourceType? ResourceType
        {
            get => _view.ResourceType;
            set
            {
                if (_view.ResourceType != value)
                {
                    OnPropertyChanging(nameof(ResourceType));
                    _view.ResourceType = value;
                    OnPropertyChanged(nameof(ResourceType));
                }
            }
        }
        public Action<InputField>? FieldsHandler
        {
            get => _view.FieldsHandler;
            set
            {
                if (_view.FieldsHandler != value)
                {
                    OnPropertyChanging(nameof(FieldsHandler));
                    _view.FieldsHandler = value;
                    OnPropertyChanged(nameof(FieldsHandler));
                }
            }
        }
        public override FrameworkElement View => _view;

        private readonly ObjectValueInput _view = new();

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _view.ValueChanged -= OnViewValueChanged;
        }

        #endregion

        #region События

        private void OnViewValueChanged(object? sender, ValueChangedEventArgs<object> e)
        {
            OnPropertyChanging(nameof(Value));
            Value = e.NewValue;
            OnPropertyChanged(nameof(Value));
        }

        #endregion
    }
}
