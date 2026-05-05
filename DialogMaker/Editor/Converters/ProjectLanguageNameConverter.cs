namespace DialogMaker.Editor
{
    public class ProjectLanguageNameConverter(ProjectController controller) : IValueConverter<ProjectLanguage, string>
    {
        private readonly ProjectController _controller = controller;

        public string Convert(ProjectLanguage Value)
        {
            return Value.Name;
        }
        public ProjectLanguage ConvertBack(string Value)
        {
            return _controller.Languages.FirstOrDefault(l => l.Name == Value)
                ?? _controller.Languages[0];
        }
    }
}
