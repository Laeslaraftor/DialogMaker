namespace DialogMaker.Editor
{
    public class ProjectLanguageNameConverter(ProjectController controller) : IValueConverter<ProjectLanguage, string>
    {
        private readonly ProjectController _controller = controller;

        public string Convert(ProjectLanguage value)
        {
            return value.Name;
        }
        public ProjectLanguage ConvertBack(string value)
        {
            return _controller.Languages.FirstOrDefault(l => l.Name == value)
                ?? _controller.Languages[0];
        }
    }
}
