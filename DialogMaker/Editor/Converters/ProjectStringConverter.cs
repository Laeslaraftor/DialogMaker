using Acly;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectStringConverter(ProjectController controller) : IValueConverter<DialogProjectString, ProjectString>
    {
        private readonly ProjectController _controller = controller;

        public ProjectString Convert(DialogProjectString Value)
        {
            return new(_controller, Value);
        }
        public DialogProjectString ConvertBack(ProjectString Value)
        {
            return Value.Original;
        }
    }
}
