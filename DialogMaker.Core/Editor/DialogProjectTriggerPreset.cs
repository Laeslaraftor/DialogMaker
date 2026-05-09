using DialogMaker.Core.Common;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Internal;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectTriggerPreset : DialogProjectResourceObject
    {
        public DialogProjectTriggerPreset(DialogProjectResources resources)
            : base(resources, Guid.NewGuid())
        {
        }
        public DialogProjectTriggerPreset(DialogProjectResources resources, DialogProjectTriggerPresetSavedState savedState)
            : base(resources, savedState)
        {
            _description = savedState.Description;

            DialogProjectTriggerPresetPort.RestoreAll(this, Inputs!, savedState.Inputs);
            DialogProjectTriggerPresetPort.RestoreAll(this, Outputs!, savedState.Outputs);
        }

        public override DialogResourceType ResourceType => DialogResourceType.TriggerPreset;
        public override bool IsSeparated => true;
        public DialogProject Project => Resources.Owner.Project;
        public string Description
        {
            get => _description ?? DefaultDescription;
            set
            {
                if (_description != value)
                {
                    OnPropertyChanging(nameof(Description));
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        public EditableCollection<DialogProjectTriggerPresetPort> Inputs
        {
            get
            {
                field ??= new(() => new(this));
                return field;
            }
        }
        public EditableCollection<DialogProjectTriggerPresetPort> Outputs
        {
            get
            {
                field ??= new(() => new(this));
                return field;
            }
        }

        private string? _description;

        #region Управление

        public void SetupNode(DialogProjectTriggerNode node)
        {
            node.TriggerId = Id;

            static void Setup(DialogProjectTriggerPresetPort preset, IEnumerable<DialogProjectNodePort> ports)
            {
                if (preset.Value == null)
                {
                    return;
                }

                foreach (var port in ports)
                {
                    if (port.Name == preset.Name && port is IValuePort valuePort && valuePort.CanPresetValue)
                    {
                        valuePort.Value = port;
                        break;
                    }
                }
            }

            foreach (var input in Inputs)
            {
                node.InputsName.Add(input.Name);
                Setup(input, node.GetInputs().Keys);
            }
            foreach (var output in Outputs)
            {
                node.InputsName.Add(output.Name);
                Setup(output, node.GetOutputs().Keys);
            }
        }
        public DialogProjectTriggerNode CreateNode(DialogProjectDialog dialog)
        {
            var node = dialog.CreateNode<DialogProjectTriggerNode>();
            SetupNode(node);

            return node;
        }

        public DialogProjectTriggerPresetPort CreateInput()
        {
            DialogProjectTriggerPresetPort port = new(this);
            Inputs.Add(port);

            return port;
        }
        public bool RemoveInput(DialogProjectTriggerPresetPort port)
        {
            return Inputs.Remove(port);
        }
        public DialogProjectTriggerPresetPort CreateOutput()
        {
            DialogProjectTriggerPresetPort port = new(this);
            Outputs.Add(port);

            return port;
        }
        public bool RemoveOutput(DialogProjectTriggerPresetPort port)
        {
            return Outputs.Remove(port);
        }

        public override IVariable ToVariable()
        {
            return new LocalVariable(Id);
        }

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectTriggerPresetSavedState()
            {
                Description = _description,
                Inputs = [.. Inputs.Select(p => p.Save())],
                Outputs = [.. Outputs.Select(p => p.Save())]
            };
        }

        #endregion

        #region Константы

        public const string DefaultDescription = "Без описания";

        #endregion
    }
}
