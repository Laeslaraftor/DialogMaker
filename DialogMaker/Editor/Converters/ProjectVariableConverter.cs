using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectVariableConverter(ProjectController controller) : IValueConverter<DialogProjectVariable, ProjectVariable>
    {
        private readonly ProjectController _controller = controller;

        public ProjectVariable Convert(DialogProjectVariable value)
        {
            return new(_controller, value);
        }
        public DialogProjectVariable ConvertBack(ProjectVariable value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
