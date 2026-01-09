using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.InputFields;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public class PropertyEditorController : Disposable
    {
        private PropertyEditorController(ObservableObject instance, PropertyInfo property, EditableTypeInfo info, InputField inputField)
        {
            ObjectInstance = instance;
            Property = property;
            Info = info;
            InputField = inputField;
            Name = info.Name;
            Description = property.GetDescription();

            inputField.CanEdit = property.CanWrite;
            inputField.Placeholder = Name;
            inputField.View.ToolTip = string.IsNullOrEmpty(Description) ? null : Description;
            inputField.Value = Value;

            instance.PropertyChanged += OnInstancePropertyChanged;
            instance.PropertyChanging += OnInstancePropertyChanging;
            inputField.PropertyChanged += OnInputFieldPropertyChanged;
        }

        public ObservableObject ObjectInstance { get; }
        public PropertyInfo Property { get; }
        public EditableTypeInfo Info { get; }
        public InputField InputField { get; }
        public string Name { get; }
        public string Description { get; }
        public object? Value
        {
            get => Property.GetValue(ObjectInstance);
            set
            {
                if (!Property.CanWrite)
                {
                    return;
                }
                if (Info.Converter != null)
                {
                    value = Info.Converter(value);
                }

                Property.SetValue(ObjectInstance, value);
            }
        }
        public FrameworkElement View => InputField.View;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            InputField.Dispose();

            ObjectInstance.PropertyChanged -= OnInstancePropertyChanged;
            ObjectInstance.PropertyChanging -= OnInstancePropertyChanging;
            InputField.PropertyChanged -= OnInputFieldPropertyChanged;
        }

        #endregion

        #region События

        private void OnInputFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var value = InputField.Value;

            if (value?.Equals(Value) != true)
            {
                Value = value;
                InvokePropertyChanged(nameof(Value));
            }
        }
        private void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != Property.Name)
            {
                return;
            }

            var value = Value;

            if (InputField.Value?.Equals(value) != true)
            {
                InputField.Value = value;
            }

            InvokePropertyChanged(nameof(Value));
        }
        private void OnInstancePropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            InvokePropertyChanging(nameof(Value));
        }

        #endregion

        #region Статика

        private static readonly List<EditableTypeInfo> _allowedTypes = new()
        {
            new(typeof(string), t =>
            {
                return new TextInputField() 
                {
                    Multiline = t.GetCustomAttribute<TextAttribute>()?.AllowMultiline == true
                };
            }),
            new(typeof(bool), t => new BoolInputField()),
            new(typeof(float), t => new FloatInputField()),
            new(typeof(int), t => new IntInputField()),
            new(typeof(Enum), t =>
            {
                return new EnumInputField()
                {
                    EnumType = t.PropertyType
                };
            }),
            new(typeof(DialogProjectReference<>), t =>
            {
                return new ReferenceInputField()
                {
                    ResourceType = t?.GetCustomAttribute<ReferenceAttribute>()?.Type
                };
            }, EditorExtensions.ToOriginalReference),
            new(t => t?.GetInterface(nameof(IEditableList)) != null, t =>
            {
                var resourceType = t?.GetCustomAttribute<ReferenceAttribute>()?.Type;
                var itemName = t?.GetCustomAttribute<ItemNameAttribute>()?.Name ?? string.Empty;
                var textSettings = t?.GetCustomAttribute<TextAttribute>();

                return new EditableListInputField()
                {
                    InputFieldHandler = field =>
                    {
                        field.Placeholder = itemName;

                        if (field is ReferenceInputField referenceField)
                        {
                            referenceField.ResourceType = resourceType;
                        }
                        if (textSettings != null && field is TextInputField textField)
                        {
                            textField.Multiline = textSettings.AllowMultiline;
                        }
                    }
                };
            }),
        };

        public static bool TryGetInfo(Type? propertyType, [NotNullWhen(true)] out EditableTypeInfo result)
        {
            result = default;

            if (propertyType == null)
            {
                return false;
            }

            foreach (var allowedType in _allowedTypes)
            {
                if (allowedType.Equals(propertyType))
                {
                    result = allowedType;
                    return true;
                }
            }

            return false;
        }
        public static bool TryGetInfo(PropertyInfo? propertyInfo, Type? propertyType, [NotNullWhen(true)] out EditableTypeInfo result)
        {
            result = default;

            if (propertyInfo == null ||
                !propertyInfo.TryGetName(out var name))
            {
                return false;
            }

            propertyType ??= propertyInfo.PropertyType;

            if (TryGetInfo(propertyType, out result))
            {
                result = new(result)
                {
                    Name = name
                };
                return true;
            }

            return false;
        }
        public static bool TryGetInfo(PropertyInfo? propertyInfo, [NotNullWhen(true)] out EditableTypeInfo result)
        {
            return TryGetInfo(propertyInfo, propertyInfo?.PropertyType, out result);
        }
        public static bool IsAvailable(Type? propertyType)
        {
            return TryGetInfo(propertyType, out _);
        }

        public static PropertyEditorController Create(ObservableObject instance, PropertyInfo property)
        {
            if (!property.CanRead)
            {
                throw new ArgumentException($"Изменение поля для типа {property.PropertyType} недоступно, так как невозможно прочесть его значение", nameof(property));
            }
            if (!TryGetInfo(property.PropertyType, out var info))
            {
                throw new ArgumentException($"Изменение поля для типа {property.PropertyType} недоступно", nameof(property));
            }

            var view = info.ViewFabric(property);
            return new(instance, property, info, view);
        }
        public static bool TryCreate(ObservableObject instance, PropertyInfo property, Type propertyType, [NotNullWhen(true)] out PropertyEditorController? result)
        {
            result = null;

            if (property.CanRead && TryGetInfo(property, propertyType, out var info))
            {
                var view = info.ViewFabric(property);
                result = new(instance, property, info, view);
                return true;
            }

            return false;
        }
        public static bool TryCreate(ObservableObject instance, PropertyInfo property, [NotNullWhen(true)] out PropertyEditorController? result)
        {
            return TryCreate(instance, property, property.PropertyType, out result);
        }
        public static List<PropertyEditorController> CreateForAllProperties(ObservableObject instance)
        {
            List<PropertyEditorController> result = [];
            SortedDictionary<SortAttribute, PropertyInfo> sortedProperties = [];
            List<PropertyInfo> otherProperties = [];

            foreach (var property in instance.GetType().GetProperties())
            {
                var sortAttribute = property.GetCustomAttribute<SortAttribute>();

                if (sortAttribute != null)
                {
                    sortedProperties.Add(sortAttribute, property);
                    continue;
                }

                otherProperties.Add(property);
            }

            List<PropertyInfo> totalProperties = [.. sortedProperties.Values];
            totalProperties.AddRange(otherProperties);

            foreach (var property in totalProperties)
            {
                if (TryCreate(instance, property, out var controller))
                {
                    result.Add(controller);
                }
            }

            return result;
        }

        #endregion
    }
}
