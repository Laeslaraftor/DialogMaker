using DialogMaker.Core.Common;
using DialogMaker.Core.Editor.Nodes;
using System;

namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectVariable : DialogProjectResourceObject, IVariable
    {
        public DialogProjectVariable(DialogProjectResources resources)
            : base(resources)
        {
            Value = ((DialogNodePortType)Type).GetDefaultValue();
        }
        public DialogProjectVariable(DialogProjectResources resources, DialogProjectVariableSavedState savedState)
            : base(resources, savedState)
        {
            Value = ConvertValue(savedState.Value);
        }

        public override DialogResourceType ResourceType => DialogResourceType.Variable;
        public abstract DialogVariableType Type { get; }
        public Type ValueType
        {
            get
            {
                field ??= ((DialogNodePortType)Type).GetDefaultType();
                return field;
            }
        }
        [Name("Значение"), Text(AllowMultiline = true)]
        public object? Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    if (!CanConvertValue(value))
                    {
                        throw new ArgumentException($"Не удалось конвертировать значение \"{value}\" для переменной типа {Type}", nameof(value));
                    }

                    InvokePropertyChanging(nameof(Value));
                    field = ConvertValue(value);
                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        public bool IsReadOnly => false;

        OperandValue IVariable.Value
        {
            get => new(Value);
            set => Value = value.Value;
        }

        #region Управление

        public override string ToString()
        {
            return $"[{Id}] {Type}: {Value}";
        }

        protected abstract bool CanConvertValue(object? value);
        protected abstract object? ConvertValue(object? value);

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectVariableSavedState()
            {
                Type = Type,
                Value = Value
            };
        }

        #endregion

        #region Константы

        public const string DefaultName = "Название переменной";

        #endregion

        #region Статика

        public static DialogProjectVariable Create(DialogProjectResources resources, DialogVariableType type)
        {
            var objType = GetVariableResourceType(type);
            return (DialogProjectVariable)Activator.CreateInstance(objType, resources); 
        }
        public static DialogProjectVariable Restore(DialogProjectResources resources, DialogProjectVariableSavedState savedState)
        {
            var objType = GetVariableResourceType(savedState.Type);
            return (DialogProjectVariable)Activator.CreateInstance(objType, resources, savedState);
        }

        private static Type GetVariableResourceType(DialogVariableType type)
        {
            var info = type.GetEnumAttribute<TypeAttribute>()?.Type;

            if (info == null)
            {
                throw new ArgumentException($"Не удалось получить тип ресурса переменной для {type}");
            }

            return info;
        }

        #endregion
    }
}
