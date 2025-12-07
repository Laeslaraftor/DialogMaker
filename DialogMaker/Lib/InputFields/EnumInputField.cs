using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.InputFields
{
    public class EnumInputField : InputField
    {
        public EnumInputField()
        {
            _view = new();
            _view.SelectionChanged += OnViewSelectionChanged;
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

                    if (_view.ToolTip?.Equals(value) != true)
                    {
                        _view.ToolTip = value;
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

                    if (value != null && _enumValues != null)
                    {
                        _view.SelectedIndex = _enumValues.IndexOf(value);
                    }

                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        public Type? EnumType
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(EnumType));
                    field = value;

                    if (value == null)
                    {
                        _enumValues = null;
                        SetItems(null);
                        Value = null;
                    }
                    else
                    {
                        _enumValues = Enum.GetValues(value);
                        SetItems(_enumValues);
                        Value = _enumValues.GetValue(0);
                    }

                    InvokePropertyChanged(nameof(EnumType));
                }
            }
        }

        public override FrameworkElement View => _view;

        private readonly ComboBox _view;
        private Array? _enumValues;

        #region Управление

        private void SetItems(Array? items)
        {
            if (items == null)
            {
                _view.ItemsSource = null;
                return;
            }

            List<string> names = new(items.Length);

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var itemName = item.GetEnumAttribute<NameAttribute>()?.Name;
                itemName ??= item.ToString() ?? string.Empty;

                names.Add(itemName);
            }

            _view.ItemsSource = names;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _view.SelectionChanged -= OnViewSelectionChanged;
        }

        #endregion

        #region События

        private void OnViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_enumValues != null &&
                _view.SelectedIndex >= 0 &&
                _view.SelectedIndex < _enumValues.Length)
            {
                Value = _enumValues.GetValue(_view.SelectedIndex);
            }
        }

        #endregion
    }
}
