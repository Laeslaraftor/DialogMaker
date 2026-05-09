using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectStringConverter(ProjectController controller) : IValueConverter<DialogProjectString, ProjectString>
    {
        private readonly ProjectController _controller = controller;

        public ProjectString Convert(DialogProjectString value)
        {
            return new(_controller, value);
        }
        public DialogProjectString ConvertBack(ProjectString value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
