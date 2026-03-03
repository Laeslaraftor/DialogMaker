using Acly;
using DialogMaker.Core.Editor.Collections;
using DialogMaker.Core.Editor.Messages;
using DialogMaker.Core.Executioning;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectTriggerNode : DialogProjectDialogNode
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
                    InvokePropertyChanging(nameof(TriggerId));
                    field = value;
                    InvokePropertyChanged(nameof(TriggerId));
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
        [NodeInput("Вход")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Выход")]
        public DialogProjectNodeOutputAction Output
        {
            get
            {
                field ??= new(this, 1);
                return field;
            }
        }
        public override bool IsUserHandleNode => true;

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

        public override void Compile(DialogCompilerContext context)
        {
            var triggerId = TriggerId;

            if (!string.IsNullOrEmpty(triggerId))
            {
                var opcode = context.Section.CreateOperation(DialogByteCode.Trigger);
                opcode.Arguments[0] = new(triggerId);
            }

            context.CompileOutputs(Output);
        }
        public override string ToString()
        {
            var id = TriggerId;

            if (string.IsNullOrEmpty(id))
            {
                return "Пустой идентификатор";
            }

            return $"Идентификатор: {id}";
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(TriggerId), TriggerId);
            savedState.Properties.TryAdd(nameof(InputsName), InputsName);
            savedState.Properties.TryAdd(nameof(OutputsName), OutputsName);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);
            TriggerId = savedState.GetProperty<string>(nameof(TriggerId));

            RestoreCollection(savedState, nameof(InputsName), InputsName);
            RestoreCollection(savedState, nameof(OutputsName), OutputsName);
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
            return name != null && !names.Contains(name);
        }
        private void AddOrRemoveMessage(EditableCollection<string?> namesToCheck, Message message, ref bool alreadyAdded)
        {
            if (ValidateNames(namesToCheck) && alreadyAdded)
            {
                alreadyAdded = false;
                InternalMessages.Remove(message);
            }
            else if (!_invalidInputNameMessageAdded)
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
                AddOrRemoveMessage(InputsName, InvalidOutputNameMessage, ref _invalidOutputNameMessageAdded);
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
        public const string InvalidInputsNameText = "Один или несколько входных параметров либо не имеют названия, либо имеют уже занятое название";
        public const string InvalidOutputsNameText = "Один или несколько выходящих значений либо не имеют названия, либо имеют уже занятое название";


        #endregion
    }
}
