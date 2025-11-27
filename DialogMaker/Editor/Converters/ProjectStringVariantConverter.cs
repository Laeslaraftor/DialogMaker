using Acly;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectStringVariantConverter(ProjectString projectString) : IValueConverter<DialogProjectStringVariant, ProjectStringVariant>
    {
        private readonly ProjectString _projectString = projectString;

        public ProjectStringVariant Convert(DialogProjectStringVariant Value)
        {
            return new(_projectString, Value);
        }
        public DialogProjectStringVariant ConvertBack(ProjectStringVariant Value)
        {
            return Value.Original;
        }
    }
}
