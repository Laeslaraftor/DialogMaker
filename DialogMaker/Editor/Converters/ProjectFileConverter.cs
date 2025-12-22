using Acly;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectFileConverter(ProjectController controller) : IValueConverter<DialogProjectItem, ProjectFile>
    {
        private readonly ProjectController _controller = controller;

        public ProjectFile Convert(DialogProjectItem Value)
        {
            return new(_controller, Value);
        }
        public DialogProjectItem ConvertBack(ProjectFile Value)
        {
            Value.Dispose();
            return Value.Original;
        }
    }
}
