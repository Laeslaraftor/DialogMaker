using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectVariableConverter(ProjectController controller) : IValueConverter<DialogProjectVariable, ProjectVariable>
    {
        private readonly ProjectController _controller = controller;

        public ProjectVariable Convert(DialogProjectVariable Value)
        {
            return new(_controller, Value);
        }
        public DialogProjectVariable ConvertBack(ProjectVariable Value)
        {
            Value.Dispose();
            return Value.Original;
        }
    }
}
