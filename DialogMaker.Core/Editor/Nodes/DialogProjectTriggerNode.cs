using DialogMaker.Core.Editor.Collections;
using DialogMaker.Core.Editor.Messages;
using System.Collections.Specialized;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectTriggerNode : DialogProjectTriggerNodeBase
    {
        public DialogProjectTriggerNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectTriggerNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Trigger;
        [Name("Идентификатор")]
        public string? TriggerId
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(TriggerId));
                    field = value;
                    OnPropertyChanged(nameof(TriggerId));
                }
            }
        }
        [Name("Входящие параметры"), ItemName("Название порта")]
        public EditableCollection<string?> InputsName
        {
            get
            {
                if (field == null)
                {
                    field = new(() => string.Empty);
                    field.CollectionChanged += OnInputsCollectionChanged;
                    field.ItemChanged += OnInputsNameItemChanged;
                }

                return field;
            }
        }
        [Name("Выходящие значения"), ItemName("Название порта")]
        public EditableCollection<string?> OutputsName
        {
            get
            {
                if (field == null)
                {
                    field = new(() => string.Empty);
                    field.CollectionChanged += OnOutputsCollectionChanged;
                    field.ItemChanged += OnOutputsNameItemChanged;
                }

                return field;
            }
        }

        protected override string? TriggerName => TriggerId;

        private Message InvalidInputNameMessage
        {
            get
            {
                if (field == null)
                {
                    MessageCommand fixCommand = new(FixCommandName, p => FixPortsName("Вход", InputsName));
                    field = new(MessageImportance.Critical, InvalidPortsNameTitle, InvalidInputsNameText, [fixCommand]);
                }

                return field;
            }
        }
        private Message InvalidOutputNameMessage
        {
            get
            {
                if (field == null)
                {
                    MessageCommand fixCommand = new(FixCommandName, p => FixPortsName("Результат", OutputsName));
                    field = new(MessageImportance.Critical, InvalidPortsNameTitle, InvalidOutputsNameText, [fixCommand]);
                }

                return field;
            }
        }


        private bool _invalidInputNameMessageAdded;
        private bool _invalidOutputNameMessageAdded;

        #region Управление

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(TriggerId), TriggerId);
            savedState.Properties.TryAdd(nameof(InputsName), InputsName);
            savedState.Properties.TryAdd(nameof(OutputsName), OutputsName);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            TriggerId = savedState.GetProperty<string>(nameof(TriggerId));

            RestoreCollection(savedState, nameof(InputsName), InputsName);
            RestoreCollection(savedState, nameof(OutputsName), OutputsName);

            base.Restore(savedState);
        }

        private void RestoreCollection(DialogProjectDialogNodeSavedState savedState, string propertyName, EditableCollection<string?> collection)
        {
            var values = savedState.GetProperty<IEnumerable<string?>>(propertyName);

            if (values != null)
            {
                foreach (var value in values)
                {
                    collection.Add(value);
                }
            }
        }

        private void FixPortsName(string baseName, EditableCollection<string?> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (ValidateName(names, names[i]))
                {
                    continue;
                }

                names[i] = GetFreeName(baseName, names, i + 1);
            }
        }
        private bool ValidateNames(EditableCollection<string?> names)
        {
            foreach (var name in names)
            {
                if (!ValidateName(names, name))
                {
                    return false;
                }
            }

            return true;
        }
        private bool ValidateName(EditableCollection<string?> names, string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            int equalsCount = 0;

            foreach (var otherName in names)
            {
                if (name.Equals(otherName))
                {
                    equalsCount++;
                }
            }

            return 1 >= equalsCount;
        }
        private void AddOrRemoveMessage(EditableCollection<string?> namesToCheck, Message message, ref bool alreadyAdded)
        {
            bool allNamesValid = ValidateNames(namesToCheck);

            if (allNamesValid && alreadyAdded)
            {
                alreadyAdded = false;
                InternalMessages.Remove(message);
            }
            else if (!allNamesValid && !alreadyAdded)
            {
                alreadyAdded = true;
                InternalMessages.Add(message);
            }
        }
        private string GetFreeName(string baseName, EditableCollection<string?> names, int index)
        {
            while (true)
            {
                string name = $"{baseName} {index}";

                if (!names.Contains(name))
                {
                    return name;
                }

                index++;
            }
        }

        #endregion

        #region События

        private void OnInputsNameItemChanged(object sender, CollectionItemEventArgs<string?> e)
        {
            if (e.Action != CollectionItemAction.Move)
            {
                AddOrRemoveMessage(InputsName, InvalidInputNameMessage, ref _invalidInputNameMessageAdded);
            }
        }
        private void OnOutputsNameItemChanged(object sender, CollectionItemEventArgs<string?> e)
        {
            if (e.Action != CollectionItemAction.Move)
            {
                AddOrRemoveMessage(OutputsName, InvalidOutputNameMessage, ref _invalidOutputNameMessageAdded);
            }
        }

        private void OnInputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateCollection(ExtraInputs, e, item =>
            {
                return new DialogProjectNodeInputValue(this, GetNextExtraPortId())
                {
                    Name = item?.ToString() ?? string.Empty
                };
            });
        }
        private void OnOutputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateCollection(ExtraOutputs, e, item =>
            {
                return new DialogProjectNodeOutputObject(this, GetNextExtraPortId())
                {
                    Name = item?.ToString() ?? string.Empty
                };
            });
        }

        private void UpdateCollection<T>(ObservableListAsDictionary<T, DialogProjectNodeMetadata> ports, NotifyCollectionChangedEventArgs e, Func<string?, T> fabric)
            where T : DialogProjectNodePort
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    T port = fabric(item?.ToString());
                    ports.Add(port, new(port, string.Empty));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var input in ports)
                {
                    input.Key.Dispose();
                }

                ports.Clear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var port = ports.ValuesCollection[e.OldStartingIndex].Key;
                port.Dispose();
                ports.ValuesCollection.RemoveAt(e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                ports.ValuesCollection[e.NewStartingIndex].Key.Name = e.NewItems[0]?.ToString() ?? string.Empty;
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var item = ports.ValuesCollection[e.OldStartingIndex];
                var destinationItem = ports.ValuesCollection[e.NewStartingIndex];

                ports.ValuesCollection[e.OldStartingIndex] = default;
                ports.ValuesCollection[e.NewStartingIndex] = default;
                ports.ValuesCollection[e.OldStartingIndex] = destinationItem;
                ports.ValuesCollection[e.NewStartingIndex] = item;
            }
        }

        #endregion

        #region Константы

        public const string FixCommandName = "Исправить";
        public const string InvalidPortsNameTitle = "Недопустимые параметры";
        public const string InvalidInputsNameText = "Входящие параметры должны иметь уникальные имена";
        public const string InvalidOutputsNameText = "Порты вывода должны иметь уникальные имена";


        #endregion
    }
}
