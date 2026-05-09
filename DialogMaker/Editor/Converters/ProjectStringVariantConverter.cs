using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectStringVariantConverter(ProjectString projectString) : IValueConverter<DialogProjectStringVariant, ProjectStringVariant>
    {
        private readonly ProjectString _projectString = projectString;

        public ProjectStringVariant Convert(DialogProjectStringVariant value)
        {
            return new(_projectString, value);
        }
        public DialogProjectStringVariant ConvertBack(ProjectStringVariant value)
        {
            return value.Original;
        }
    }
}
