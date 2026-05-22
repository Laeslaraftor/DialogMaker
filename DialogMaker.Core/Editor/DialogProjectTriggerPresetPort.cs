using System.ComponentModel;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectTriggerPresetPort : Disposable
    {
        public DialogProjectTriggerPresetPort(DialogProjectTriggerPreset triggerPreset)
        {
            TriggerPreset = triggerPreset;
            triggerPreset.PropertyChanged += OnTriggerPresetPropertyChanged;
        }
        public DialogProjectTriggerPresetPort(DialogProjectTriggerPreset triggerPreset, DialogProjectTriggerPresetPortSavedState savedState)
            : this(triggerPreset)
        {
            _name = savedState.Name;
            Value = savedState.Value;
            ValueType = savedState.ValueType;
        }

        public DialogProjectTriggerPreset TriggerPreset { get; }
        public string? Name
        {
            get => _name ?? DefaultName;
            set
            {
                if (_name != value)
                {
                    OnPropertyChanging(nameof(Name));
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        [AllowedTypes(AllowedObjectValues.AllWithoutList)]
        public object? Value
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
        public AllowedObjectValues ValueType
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(ValueType));
                    field = value;
                    UpdateAllowedValues(value);
                    OnPropertyChanged(nameof(ValueType));
                }
            }
        } = AllowedObjectValues.Number;
        public AllowedObjectValues AllowedValues
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(AllowedValues));
                    field = value;
                    OnPropertyChanged(nameof(AllowedValues));
                }
            }
        } = GetAllowedValues(AllowedObjectValues.Number);

        private string? _name;

        #region Управление

        public DialogProjectTriggerPresetPortSavedState Save()
        {
            return new()
            {
                Name = _name,
                Value = Value,
                ValueType = ValueType
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            TriggerPreset.PropertyChanged -= OnTriggerPresetPropertyChanged;
        }

        private void UpdateAllowedValues(AllowedObjectValues valueType)
        {
            AllowedValues = GetAllowedValues(valueType);
        }

        #endregion

        #region Константы

        public const string DefaultName = "Название порта";

        #endregion

        #region События

        private void OnTriggerPresetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TriggerPreset));
        }

        #endregion

        #region Статика

        public static void RestoreAll(DialogProjectTriggerPreset triggerPreset, IList<DialogProjectTriggerPresetPort> buffer, IEnumerable<DialogProjectTriggerPresetPortSavedState>? savedStates)
        {
            if (savedStates == null)
            {
                return;
            }

            foreach (var savedState in savedStates)
            {
                try
                {
                    buffer.Add(new(triggerPreset, savedState));
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
        }

        private static AllowedObjectValues GetAllowedValues(AllowedObjectValues valueType)
        {
            AllowedObjectValues result = AllowedObjectValues.Resource;

            if (valueType == AllowedObjectValues.Resource)
            {
                return result;
            }
            if (valueType == AllowedObjectValues.Number)
            {
                result |= AllowedObjectValues.Number;
            }
            else if (valueType == AllowedObjectValues.Bool)
            {
                result |= AllowedObjectValues.Bool;
            }
            else if (valueType == AllowedObjectValues.String)
            {
                result |= AllowedObjectValues.String;
            }

            return result;
        }

        #endregion
    }
}
