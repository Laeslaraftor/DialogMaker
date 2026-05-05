using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectEmotionConverter(ProjectController controller) : IValueConverter<DialogProjectEmotion, ProjectEmotion>
    {
        private readonly ProjectController _controller = controller;

        public ProjectEmotion Convert(DialogProjectEmotion Value)
        {
            return new(_controller, Value);
        }
        public DialogProjectEmotion ConvertBack(ProjectEmotion Value)
        {
            var original = Value.Original;
            Value.Dispose();

            return original;
        }
    }
}
