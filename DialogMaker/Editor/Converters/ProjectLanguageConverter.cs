using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Lib.Elements;

namespace DialogMaker.Editor
{
    public class ProjectLanguageConverter(ProjectController controller) : IValueConverter<DialogProjectLanguage, ProjectLanguage>, IDisposable
    {
        private readonly ElementsPool<ProjectLanguage> _languages = new(() => new(controller));

        public ProjectLanguage Convert(DialogProjectLanguage Value)
        {
            var result = _languages.GetElement();
            result.Language = Value;

            return result;
        }
        public DialogProjectLanguage ConvertBack(ProjectLanguage Value)
        {
            if (Value.Language == null)
            {
                throw new ArgumentException("Язык не указан", nameof(Value));
            }

            var language = Value.Language;
            Value.Language = null;

            _languages.Free(Value);

            return language;
        }

        public void Dispose()
        {
            _languages.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
