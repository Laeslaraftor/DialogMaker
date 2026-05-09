using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectPackConverter(ProjectController controller) : IValueConverter<DialogProjectPack, ProjectPack>
    {
        private readonly ProjectController _controller = controller;

        public ProjectPack Convert(DialogProjectPack value)
        {
            return new(_controller, value);
        }
        public DialogProjectPack ConvertBack(ProjectPack value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
