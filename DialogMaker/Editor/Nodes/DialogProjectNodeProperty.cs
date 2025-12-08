using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib.InputFields;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace DialogMaker.Editor
{
    public class DialogProjectNodeProperty : ObservableObject, IDisposable
    {
        protected DialogProjectNodeProperty(DialogProjectNode node, PropertyInfo property, AllowedType typeInfo)
        {
            InputField = typeInfo.ViewFabric(property);
            Node = node;
            Property = property;
            Name = property.GetName();
            Description = property.GetDescription();
            InputField.Placeholder = Name;
            InputField.Value = Value;
            TypeInfo = typeInfo;

            node.Original.PropertyChanged += OnOriginalNodePropertyChanged;
            node.Original.PropertyChanging += OnOriginalNodePropertyChanging;
            InputField.PropertyChanged += OnInputFieldPropertyChanged;
        }
        ~DialogProjectNodeProperty()
        {
            Dispose(false);
        }

        public DialogProjectNode Node { get; }
        public string Name { get; }
        public string Description { get; }
        public object? Value
        {
            get => Property.GetValue(Node.Original);
            set
            {
                if (TypeInfo.Converter != null)
                {
                    value = TypeInfo.Converter(value);
                }

                Property.SetValue(Node.Original, value);
            }
        }
        public FrameworkElement View => InputField.View;

        protected PropertyInfo Property { get; }
        protected InputField InputField { get; }
        protected AllowedType TypeInfo { get; }

        #region Управление

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            Node.Original.PropertyChanged -= OnOriginalNodePropertyChanged;
            Node.Original.PropertyChanging -= OnOriginalNodePropertyChanging;
            InputField.PropertyChanged -= OnInputFieldPropertyChanged;

            InputField.Dispose();
        }

        #endregion

        #region События

        protected virtual void OnValueChanging()
        {
            InvokePropertyChanging(nameof(Value));
        }
        protected virtual void OnValueChanged()
        {
            InvokePropertyChanged(nameof(Value));

            var value = Value;

            if (InputField.Value?.Equals(value) != true)
            {
                InputField.Value = value;
            }
        }

        private void OnInputFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var value = InputField.Value;

            if (Value?.Equals(value) != true)
            {
                Value = value;
            }
        }
        protected virtual void OnOriginalNodePropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == Property.Name)
            {
                OnValueChanging();
            }
        }
        protected virtual void OnOriginalNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Property.Name)
            {
                OnValueChanged();
            }
        }

        #endregion

        #region Статика

        private static readonly List<AllowedType> _allowedTypes = new()
        {
            new(typeof(string), t => new TextInputField()),
            new(typeof(bool), t => new BoolInputField()),
            //new(typeof(float), t => new TextInputField()),
            //new(typeof(int), t => new TextInputField()),
            new(typeof(Enum), t => new EnumInputField()),
            new(typeof(DialogProjectReference<>), t =>
            {
                return new ReferenceInputField()
                {
                    ResourceType = t?.GetCustomAttribute<ReferenceAttribute>()?.Type
                };
            }, EditorExtensions.ToOriginalReference),
        };

        public static List<DialogProjectNodeProperty> GetProperties(DialogProjectNode node)
        {
            List<DialogProjectNodeProperty> result = [];
            var properties = node.Original.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (!property.CanWrite || !property.CanWrite)
                {
                    continue;
                }

                foreach (var typeInfo in _allowedTypes)
                {
                    if (typeInfo.Equals(property.PropertyType))
                    {
                        result.Add(new(node, property, typeInfo));
                    }
                }
            }

            return result;
        }

        #endregion

        #region Классы

        protected readonly struct AllowedType(Type type, Func<MemberInfo, InputField> viewFabric, Func<object?, object?>? converter = null)
            : IEquatable<Type>
        {
            public Type Type { get; } = type;
            public Func<MemberInfo, InputField> ViewFabric { get; } = viewFabric;
            public Func<object?, object?>? Converter { get; } = converter;

            public readonly bool Equals(Type? other)
            {
                return other != null &&
                       (Type == other ||
                       Type.IsEnum && other.IsEnum ||
                       Type.Name.Contains(other.Name));
            }
        }

        #endregion
    }
}
