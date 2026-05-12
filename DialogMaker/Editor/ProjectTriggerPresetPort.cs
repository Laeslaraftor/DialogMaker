using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using System.ComponentModel;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public class ProjectTriggerPresetPort : Disposable
    {
        public ProjectTriggerPresetPort(ProjectTriggerPreset trigger, DialogProjectTriggerPresetPort original)
        {
            TriggerPreset = trigger;
            Original = original;
            ContextMenu = new TriggerPresetPortContextMenu(this);

            original.PropertyChanged += OnOriginalPropertyChanged;
            original.PropertyChanging += OnOriginalPropertyChanging;
        }

        public ProjectTriggerPreset TriggerPreset { get; }
        public DialogProjectTriggerPresetPort Original { get; }
        public string? Name
        {
            get => Original.Name;
            set => Original.Name = value;
        }
        public object? Value
        {
            get => Original.Value;
            set => Original.Value = value;
        }
        public AllowedObjectValues ValueType
        {
            get => Original.ValueType;
            set => Original.ValueType = value;
        }
        public AllowedObjectValues AllowedValues => Original.AllowedValues;
        public ContextMenu ContextMenu { get; }

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Original.PropertyChanged += OnOriginalPropertyChanged;
            Original.PropertyChanging += OnOriginalPropertyChanging;
        }

        #endregion

        #region События

        private void OnOriginalPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            OnPropertyChanging(e);
        }
        private void OnOriginalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        #endregion
    }
}
