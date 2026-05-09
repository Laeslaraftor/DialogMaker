using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectCharacterConverter(ProjectController controller) : IValueConverter<DialogProjectCharacter, ProjectCharacter>
    {
        private readonly ProjectController controller = controller;

        public ProjectCharacter Convert(DialogProjectCharacter value)
        {
            return new(controller, value);
        }
        public DialogProjectCharacter ConvertBack(ProjectCharacter value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
