using System;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectStringVariant : ObservableObject, ISavable
    {
        public DialogProjectStringVariant(DialogProjectString replica)
        {
            String = replica;
        }
        public DialogProjectStringVariant(DialogProjectString replica, DialogProjectStringVariantSavedState savedState)
            : this(replica)
        {
            Text = savedState.Text;

            if (savedState.LanguageId != null && 
                Guid.TryParse(savedState.LanguageId, out var languageId) &&
                replica.Resources.Owner.Project.TryGetLanguage(languageId, out var language))
            {
                _language = language;
            }
            if (savedState.VoiceId != null)
            {
                try
                {
                    Voice = DialogProjectReference<DialogProjectItem>.Restore(replica.Resources.Owner.Project, savedState.VoiceId);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
        }

        public DialogProjectString String { get; }
        public DialogProjectLanguage? Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    InvokePropertyChanging(nameof(Language));
                    _language = value;
                    InvokePropertyChanged(nameof(Language));
                }
            }
        }
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    InvokePropertyChanging(nameof(Text));
                    _text = value;
                    InvokePropertyChanged(nameof(Text));
                }
            }
        }
        public DialogProjectReference<DialogProjectItem>? Voice
        {
            get => _voice;
            set
            {
                if (_voice != value)
                {
                    InvokePropertyChanging(nameof(Voice)); 
                    _voice = value;
                    InvokePropertyChanged(nameof(Voice));
                }
            }
        }

        private DialogProjectLanguage? _language;
        private string _text = string.Empty;
        private DialogProjectReference<DialogProjectItem>? _voice;

        #region Управление

        public ISavedState Save()
        {
            return new DialogProjectStringVariantSavedState()
            {
                LanguageId = Language?.ProjectId.ToString(),
                Text = Text?.Trim() ?? string.Empty,
                VoiceId = Voice?.Save()
            };
        }

        public override string ToString()
        {
            return Text;
        }

        #endregion
    }
}
