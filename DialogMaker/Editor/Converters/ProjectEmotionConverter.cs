using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectEmotionConverter(ProjectController controller) : IValueConverter<DialogProjectEmotion, ProjectEmotion>
    {
        private readonly ProjectController _controller = controller;

        public ProjectEmotion Convert(DialogProjectEmotion value)
        {
            return new(_controller, value);
        }
        public DialogProjectEmotion ConvertBack(ProjectEmotion value)
        {
            var original = value.Original;
            value.Dispose();

            return original;
        }
    }
}
