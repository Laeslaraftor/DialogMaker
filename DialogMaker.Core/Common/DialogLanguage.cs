using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogLanguage
    {
        public DialogLanguage(DialogPackage package, DialogProjectLanguage language)
        {
            Package = package;
            Id = language.Id;
            Name = language.Name;
        }
        public DialogLanguage(DialogPackage package, DialogLanguageSavedState savedState)
        {
            Package = package;
            Id = savedState.Id;
            Name = savedState.Name;
        }

        public DialogPackage Package { get; }
        public string Id { get; }
        public string Name { get; }

        #region Управление

        public DialogLanguageSavedState Save()
        {
            return new()
            {
                Id = Id,
                Name = Name
            };
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
