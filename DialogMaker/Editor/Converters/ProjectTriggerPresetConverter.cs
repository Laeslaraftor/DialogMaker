using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectTriggerPresetConverter(ProjectController controller) : IValueConverter<DialogProjectTriggerPreset, ProjectTriggerPreset>
    {
        private readonly ProjectController _controller = controller;

        public ProjectTriggerPreset Convert(DialogProjectTriggerPreset value)
        {
            return new(_controller, value);
        }
        public DialogProjectTriggerPreset ConvertBack(ProjectTriggerPreset value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
