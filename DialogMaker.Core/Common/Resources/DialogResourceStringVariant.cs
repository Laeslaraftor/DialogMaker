using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceStringVariant
    {
        public DialogResourceStringVariant(DialogResourceString str, DialogProjectStringVariant variant)
        {
            String = str;
            Value = variant.Text;

            if (variant.Language != null)
            {
                Language = str.Resources.Package.Languages[variant.Language.Id];
            }
            if (variant.Voice != null)
            {
                var item = variant.Voice.Resolve();
                _voicePath = new(variant.Voice.ResourcesPath, item.Id);
            }
        }
        public DialogResourceStringVariant(DialogResourceString str, DialogResourceStringVariantSavedState savedState)
        {
            String = str;
            Value = savedState.Value;

            if (savedState.Language != null)
            {
                Language = str.Resources.Package.Languages[savedState.Language];
            }
            if (savedState.Voice != null && ResourcePath.TryParse(savedState.Voice, out var voicePath))
            {
                _voicePath = voicePath;
            }
        }

        public DialogResourceString String { get; }
        public DialogLanguage? Language { get; }
        public string Value { get; }
        public DialogResourceFile? Voice
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                if (_voicePath.IsEmpty)
                {
                    return null;
                }

                field ??= String.Resources.Package.GetResource<DialogResourceFile>(_voicePath);

                return field;
            }
        }

        private readonly ResourcePath _voicePath;

        #region Управление

        public DialogResourceStringVariantSavedState Save()
        {
            DialogResourceStringVariantSavedState result = new()
            {
                Language = Language?.Id,
                Value = Value
            };

            if (!_voicePath.IsEmpty)
            {
                result.Voice = _voicePath;
            }

            return result;
        }

        #endregion
    }
}
