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
            if (variant.Voice == null)
            {
                return;
            }

            var item = variant.Voice.Resolve();

            if (str.Resources.Files.TryGetValue(item.Id, out var file))
            {
                Voice = file;
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
            if (savedState.VoiceId != null &&
                str.Resources.Files.TryGetValue(savedState.VoiceId, out var file))
            {
                Voice = file;
            }
        }

        public DialogResourceString String { get; }
        public DialogLanguage? Language { get; }
        public string Value { get; }
        public DialogResourceFile? Voice { get; }

        #region Управление

        public DialogResourceStringVariantSavedState Save()
        {
            return new()
            {
                Language = Language?.Id,
                VoiceId = Voice?.Id,
                Value = Value
            };
        }

        #endregion
    }
}
