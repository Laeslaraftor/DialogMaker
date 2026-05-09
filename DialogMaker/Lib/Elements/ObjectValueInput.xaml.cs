using Acly.Tokens;
using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.InputFields;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class ObjectValueInput : UserControl, IReferenceView
    {
        public ObjectValueInput()
        {
            InitializeComponent();

            _typesSelector.ItemsSource = _valuesTypeInfo;
            SetAllowedValues(AllowedObjectValues.All);
        }
        ~ObjectValueInput()
        {
            foreach (var field in _createdFields)
            {
                field.Value.Dispose();
            }

            Dispatcher.Invoke(() =>
            {
                _createdFields.Clear();
                _valuesTypeInfo.Clear();
            });
        }

        public event EventHandler<ValueChangedEventArgs<object>>? ValueChanged;

        public AllowedObjectValues AllowedValues
        {
            get => (AllowedObjectValues)GetValue(AllowedValuesProperty);
            set => SetValue(AllowedValuesProperty, value);
        }
        public AllowedObjectValues SelectedValueType
        {
            get => (AllowedObjectValues)GetValue(SelectedValueTypeProperty);
            set => SetValue(SelectedValueTypeProperty, value);
        }
        public DialogResourceType? ResourceType
        {
            get => (DialogResourceType?)GetValue(ResourceTypeProperty);
            set => SetValue(ResourceTypeProperty, value);
        }
        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public Action<InputField>? FieldsHandler
        {
            get => GetValue(FieldsHandlerProperty) as Action<InputField>;
            set => SetValue(FieldsHandlerProperty, value);
        }

        bool IReferenceView.CanSetReference => AllowedValues.HasFlag(AllowedObjectValues.Resource);
        ProjectResourceItem? IReferenceView.Item
        {
            get => Value as ProjectResourceItem;
            set => Value = value;
        }
        DialogResourceType? IReferenceView.RequestedResourceType
        {
            get => ResourceType;
            set => ResourceType = value;
        }
        FrameworkElement IReferenceView.View => this;

        private readonly ObservableCollection<ValueTypeInfo> _valuesTypeInfo = [];
        private readonly Dictionary<AllowedObjectValues, InputField> _createdFields = [];
        private bool _skipNextSync;
        private ValueTypeInfo? _lastSelectedValueType;
        private Token? _valuesSyncToken;

        #region Управление

        Point IReferenceView.GetPosition(Visual relativeTo)
        {
            if (_createdFields.TryGetValue(AllowedObjectValues.Resource, out var info))
            {
                return info.View.GetPosition(relativeTo);
            }

            return LibExtensions.GetPosition(this, relativeTo);
        }

        private async void SetAllowedValues(AllowedObjectValues allowedValues)
        {
            var resourceType = ResourceType;
            var placeholder = Placeholder;
            var selectedValue = _lastSelectedValueType;
            var selectedIndex = -1;
            Token currentToken = new();
            _valuesSyncToken = currentToken;

            if (_lastSelectedValueType != null)
            {
                selectedIndex = _valuesTypeInfo.IndexOf(_lastSelectedValueType.Value);
            }

            var fieldsHandler = FieldsHandler;

            _valuesTypeInfo.Clear();

            foreach (var allowedValue in allowedValues.GetValues())
            {
                var field = GetOrCreateField(allowedValue);

                if (field == null)
                {
                    continue;
                }

                SetResourceType(field, resourceType);

                var name = allowedValue.GetEnumAttribute<NameAttribute>()?.Name;
                name ??= allowedValue.ToString();
                field.Placeholder = placeholder;

                fieldsHandler?.Invoke(field);

                _valuesTypeInfo.Add(new()
                {
                    Name = name,
                    AllowedValueFlag = allowedValue,
                    Field = field
                });
            }

            int oldSelectedValueIndex = 0;

            if (selectedValue != null)
            {
                oldSelectedValueIndex = _valuesTypeInfo.IndexOf(selectedValue.Value);
            }

            if (oldSelectedValueIndex != selectedIndex)
            {
                await Task.Delay(10);

                if (_valuesSyncToken == currentToken)
                {
                    _typesSelector.SelectedIndex = oldSelectedValueIndex;
                }
            }
        }
        private void SetResourceType(DialogResourceType? resourceType)
        {
            foreach (var field in _createdFields.Values)
            {
                SetResourceType(field, resourceType);
            }
        }
        private void SetResourceType(InputField field, DialogResourceType? resourceType)
        {
            if (field is ReferenceInputField referenceField)
            {
                referenceField.ResourceType = resourceType;
            }
        }

        private InputField? GetOrCreateField(AllowedObjectValues allowedValue)
        {
            if (_createdFields.TryGetValue(allowedValue, out var result))
            {
                return result;
            }

            var typeAttribute = allowedValue.GetEnumAttribute<TypeAttribute>();

            if (typeAttribute == null)
            {
                return null;
            }

            result = InputField.GetField(typeAttribute.Type);
            _createdFields.Add(allowedValue, result);

            if (result is EditableListInputField listField)
            {
                listField.Value = new EditableCollection<object?>(() => null);
                listField.InputFieldHandler = field =>
                {
                    if (field is ObjectInputField objectField)
                    {
                        objectField.AllowedValues = AllowedValues;
                        objectField.ResourceType = ResourceType;
                        objectField.FieldsHandler = FieldsHandler;
                    }

                    FieldsHandler?.Invoke(field);
                };
            }

            result.View.Visibility = Visibility.Collapsed;
            _inputsContainer.Children.Add(result.View);

            return result;
        }
        private ValueTypeInfo? ShowType(int index)
        {
            if (_valuesTypeInfo.Count == 0)
            {
                return null;
            }
            else if (0 > index || index >= _valuesTypeInfo.Count)
            {
                index = 0;
            }

            _valuesSyncToken = null;
            int i = 0;
            ValueTypeInfo? result = null;

            foreach (var info in _valuesTypeInfo)
            {
                info.Field.PropertyChanged -= OnFieldPropertyChanged;
                Visibility visibility = Visibility.Collapsed;

                if (i == index)
                {
                    visibility = Visibility.Visible;
                    info.Field.PropertyChanged += OnFieldPropertyChanged;
                    result = info;
                }

                info.Field.View.Visibility = visibility;
                i++;
            }

            _typesSelector.SelectedIndex = index;

            return result;
        }
        private void SetValueType(AllowedObjectValues type)
        {
            int index = 0;

            foreach (var info in _valuesTypeInfo)
            {
                if (info.AllowedValueFlag == type)
                {
                    ShowType(index);
                    return;
                }

                index++;
            }
        }

        #endregion

        #region События

        private void OnTypesSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var info = ShowType(_typesSelector.SelectedIndex);

            if (info == null)
            {
                return;
            }

            _skipNextSync = true;
            _lastSelectedValueType = info.Value;
            Value = info.Value.Field.Value;
            SelectedValueType = info.Value.AllowedValueFlag;
            _skipNextSync = false;
        }

        private void OnFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value) && sender is InputField field)
            {
                Value = field.Value;
            }
        }

        private static void OnAllowedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ObjectValueInput view)
            {
                view.SetAllowedValues((AllowedObjectValues)e.NewValue);
            }
        }
        private static void OnSelectedValueTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ObjectValueInput view)
            {
                view.SetValueType((AllowedObjectValues)e.NewValue);
            }
        }
        private static void OnResourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ObjectValueInput view)
            {
                view.SetResourceType((DialogResourceType?)e.NewValue);
            }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ObjectValueInput view)
            {
                return;
            }
            if (!view._skipNextSync)
            {
                var valueType = DialogProjectNodeInput.GetValueType(e.NewValue);
                ValueTypeInfo? valueTypeInfo = null;

                if (valueType != null)
                {
                    for (int i = 0; i < view._valuesTypeInfo.Count; i++)
                    {
                        var info = view._valuesTypeInfo[i];

                        if (info.AllowedValueFlag == valueType)
                        {
                            valueTypeInfo = info;
                            info.Field.Value = e.NewValue;
                            view.ShowType(i);
                            break;
                        }
                    }
                }
                else
                {
                    var selectedIndex = view._typesSelector.SelectedIndex;

                    if (selectedIndex >= 0 && selectedIndex < view._valuesTypeInfo.Count)
                    {
                        valueTypeInfo = view._valuesTypeInfo[selectedIndex];
                    }
                }

                if (valueTypeInfo != null)
                {
                    view._lastSelectedValueType = valueTypeInfo;
                    valueTypeInfo.Value.Field.Value = e.NewValue;
                }
                else if (view._valuesTypeInfo.Count > 0)
                {
                    view._valuesTypeInfo[0].Field.Value = null;
                }
            }

            view.ValueChanged?.Invoke(view, e);
        }
        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ObjectValueInput view)
            {
                return;
            }

            var placeholder = e.NewValue.ToString() ?? string.Empty;

            foreach (var field in view._createdFields.Values)
            {
                field.Placeholder = placeholder;
            }
        }
        private static void OnFieldsHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ObjectValueInput view)
            {
                return;
            }

            var handler = e.NewValue as Action<InputField>;

            foreach (var field in view._createdFields.Values)
            {
                if (field is EditableListInputField listField)
                {
                    listField.InputFieldHandler = handler;
                }

                handler?.Invoke(field);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty AllowedValuesProperty = DependencyProperty.Register(nameof(AllowedValues), typeof(AllowedObjectValues),
            typeof(ObjectValueInput), new(AllowedObjectValues.All, OnAllowedValuesChanged));
        public static readonly DependencyProperty SelectedValueTypeProperty = DependencyProperty.Register(nameof(SelectedValueType), typeof(AllowedObjectValues),
            typeof(ObjectValueInput), new(AllowedObjectValues.All, OnSelectedValueTypeChanged));
        public static readonly DependencyProperty ResourceTypeProperty = DependencyProperty.Register(nameof(ResourceType), typeof(DialogResourceType?),
            typeof(ObjectValueInput), new(null, OnResourceTypeChanged));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object),
            typeof(ObjectValueInput), new(null, OnValueChanged));
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(ObjectValueInput), new(string.Empty, OnPlaceholderChanged));
        public static readonly DependencyProperty FieldsHandlerProperty = DependencyProperty.Register(nameof(FieldsHandler), typeof(Action<InputField>),
            typeof(ObjectValueInput), new(null, OnFieldsHandlerChanged));

        #endregion

        #region Классы

        private struct ValueTypeInfo : IEquatable<ValueTypeInfo>
        {
            public string Name { get; set; }
            public AllowedObjectValues AllowedValueFlag { get; set; }
            public InputField Field { get; set; }

            public bool Equals(ValueTypeInfo other)
            {
                return Name == other.Name &&
                       AllowedValueFlag == other.AllowedValueFlag &&
                       Field == other.Field;
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                return obj is ValueTypeInfo info && Equals(info);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(Name, AllowedValueFlag, Field);
            }
        }

        #endregion
    }
}
