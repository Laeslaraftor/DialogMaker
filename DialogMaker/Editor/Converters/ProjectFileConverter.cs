using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectFileConverter(ProjectController controller) : IValueConverter<DialogProjectItem, ProjectFile>
    {
        private readonly ProjectController _controller = controller;

        public ProjectFile Convert(DialogProjectItem value)
        {
            return new(_controller, value);
        }
        public DialogProjectItem ConvertBack(ProjectFile value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
