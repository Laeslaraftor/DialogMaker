using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectCharacterConverter(ProjectController controller) : IValueConverter<DialogProjectCharacter, ProjectCharacter>
    {
        private readonly ProjectController controller = controller;

        public ProjectCharacter Convert(DialogProjectCharacter Value)
        {
            return new(controller, Value);
        }
        public DialogProjectCharacter ConvertBack(ProjectCharacter Value)
        {
            Value.Dispose();
            return Value.Original;
        }
    }
}
