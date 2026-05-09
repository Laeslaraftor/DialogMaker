using DialogMaker.Core.Editor;
using DialogMaker.Lib.Elements;

namespace DialogMaker.Editor
{
    public class ProjectLanguageConverter(ProjectController controller) : IValueConverter<DialogProjectLanguage, ProjectLanguage>, IDisposable
    {
        private readonly ElementsPool<ProjectLanguage> _languages = new(() => new(controller));

        public ProjectLanguage Convert(DialogProjectLanguage value)
        {
            var result = _languages.GetElement();
            result.Language = value;

            return result;
        }
        public DialogProjectLanguage ConvertBack(ProjectLanguage value)
        {
            if (value.Language == null)
            {
                throw new ArgumentException("Язык не указан", nameof(value));
            }

            var language = value.Language;
            value.Language = null;

            _languages.Free(value);

            return language;
        }

        public void Dispose()
        {
            _languages.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
