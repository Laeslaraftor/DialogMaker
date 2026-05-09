using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectTriggerPresetPortConverter(ProjectTriggerPreset trigger) : IValueConverter<DialogProjectTriggerPresetPort, ProjectTriggerPresetPort>
    {
        private readonly ProjectTriggerPreset _trigger = trigger;

        public ProjectTriggerPresetPort Convert(DialogProjectTriggerPresetPort value)
        {
            return new(_trigger, value);
        }
        public DialogProjectTriggerPresetPort ConvertBack(ProjectTriggerPresetPort value)
        {
            return value.Original;
        }
    }
}
