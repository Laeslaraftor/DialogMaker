using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectTriggerPreset : ProjectResourceItem<DialogProjectTriggerPreset>
    {
        public ProjectTriggerPreset(ProjectController project, DialogProjectTriggerPreset original) : base(project, original)
        {
            CreateInputCommand = new RelayCommand(_ => original.CreateInput());
            RemoveInputCommand = new RelayCommand(ExecuteRemoveInput);
            CreateOutputCommand = new RelayCommand(_ => original.CreateOutput());
            RemoveOutputCommand = new RelayCommand(ExecuteRemoveOutput);

            ProjectTriggerPresetPortConverter converter = new(this);

            _inputsSync = new(original.Inputs, _inputs, converter);
            _outputsSync = new(original.Outputs, _outputs, converter);

            Inputs = new(_inputs);
            Outputs = new(_outputs);
        }

        public ReferenceReadOnlyList<ProjectTriggerPresetPort> Inputs { get; }
        public ReferenceReadOnlyList<ProjectTriggerPresetPort> Outputs { get; }
        public ICommand CreateInputCommand { get; }
        public ICommand RemoveInputCommand { get; }
        public ICommand CreateOutputCommand { get; }
        public ICommand RemoveOutputCommand { get; }

        private readonly EditableCollection<ProjectTriggerPresetPort> _inputs = [];
        private readonly EditableCollection<ProjectTriggerPresetPort> _outputs = [];
        private readonly CollectionSynchronizer<DialogProjectTriggerPresetPort, ProjectTriggerPresetPort> _inputsSync;
        private readonly CollectionSynchronizer<DialogProjectTriggerPresetPort, ProjectTriggerPresetPort> _outputsSync;

        #region Управление

        public bool Contains(ProjectTriggerPresetPort port)
        {
            return _inputs.Contains(port) || _outputs.Contains(port);
        }
        public bool Remove(ProjectTriggerPresetPort port)
        {
            return Original.RemoveInput(port.Original) || Original.RemoveOutput(port.Original);
        }

        public override ItemContextMenu CreateContextMenu()
        {
            return new TriggerPresetContextMenu(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _inputsSync.Dispose();
            _outputsSync.Dispose();
        }

        #endregion

        #region Команды

        private static void ExecuteRemoveInput(object? parameter)
        {
            if (parameter is DialogProjectTriggerPresetPort port)
            {
                port.TriggerPreset.RemoveInput(port);
            }
        }
        private static void ExecuteRemoveOutput(object? parameter)
        {
            if (parameter is DialogProjectTriggerPresetPort port)
            {
                port.TriggerPreset.RemoveOutput(port);
            }
        }

        #endregion
    }
}
