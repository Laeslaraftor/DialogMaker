using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public abstract class DialogResourceVariable : DialogResourceObject, IVariable
    {
        public DialogResourceVariable(DialogResources resources, DialogProjectVariable variable)
            : base(resources, variable)
        {
            Value = new(variable.Value);
        }
        public DialogResourceVariable(DialogResources resources, DialogResourceVariableSavedState savedState)
            : base(resources, savedState)
        {
            Value = new(savedState.Value);
        }

        public override DialogResourceType ResourceType => DialogResourceType.Variable;
        public abstract DialogVariableType Type { get; }
        public OperandValue Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Value));
                    field = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
        public bool IsReadOnly => false;

        #region Управление

        public override IVariable ToVariable()
        {
            return this;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        protected abstract object ConvertValue(object? value);

        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            return new DialogResourceVariableSavedState()
            {
                Type = Type,
                Value = Value.Value
            };
        }

        #endregion

        #region Статика

        public static DialogResourceVariable Create(DialogResources resources, DialogProjectVariable variable)
        {
            return variable.Type switch
            {
                DialogVariableType.Number => new DialogResourceNumberVariable(resources, variable),
                DialogVariableType.Bool => new DialogResourceBoolVariable(resources, variable),
                DialogVariableType.String => new DialogResourceStringVariable(resources, variable),
                _ => throw new ArgumentException($"Неизвестный тип переменной: {variable.Type}", nameof(variable)),
            };
        }
        public static DialogResourceVariable Create(DialogResources resources, DialogResourceVariableSavedState savedState)
        {
            return savedState.Type switch
            {
                DialogVariableType.Number => new DialogResourceNumberVariable(resources, savedState),
                DialogVariableType.Bool => new DialogResourceBoolVariable(resources, savedState),
                DialogVariableType.String => new DialogResourceStringVariable(resources, savedState),
                _ => throw new ArgumentException($"Неизвестный тип переменной: {savedState.Type}", nameof(savedState)),
            };
        }

        #endregion
    }
}
