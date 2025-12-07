using Acly;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectPackConverter(ProjectController controller) : IValueConverter<DialogProjectPack, ProjectPack>
    {
        private readonly ProjectController _controller = controller;

        public ProjectPack Convert(DialogProjectPack Value)
        {
            return new(_controller, Value);
        }
        public DialogProjectPack ConvertBack(ProjectPack Value)
        {
            Value.Dispose();
            return Value.Original;
        }
    }
}
