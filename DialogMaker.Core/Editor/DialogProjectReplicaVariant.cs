using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReplicaVariant : ObservableObject, ISavable
    {
        public DialogProjectReplicaVariant(DialogProjectReplica replica)
        {
            Replica = replica;
        }
        public DialogProjectReplicaVariant(DialogProjectReplica replica, DialogProjectReplicaVariantSavedState savedState)
            : this(replica)
        {
            Text = savedState.Text;

            if (savedState.LanguageId != null && 
                Guid.TryParse(savedState.LanguageId, out var languageId) &&
                replica.Resources.Owner.Project.TryGetLanguage(languageId, out var language))
            {
                _language = language;
            }
        }

        public DialogProjectReplica Replica { get; }
        public DialogProjectLanguage? Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
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
                    _text = value;
                    InvokePropertyChanged(nameof(Text));
                }
            }
        }
        public DialogProjectResourceItem? Voice
        {
            get => _voice;
            set
            {
                if (_voice != value)
                {
                    _voice = value;
                    InvokePropertyChanged(nameof(Voice));
                }
            }
        }

        private DialogProjectLanguage? _language;
        private string _text = string.Empty;
        public DialogProjectResourceItem? _voice;

        #region Управление

        public ISavedState Save()
        {
            return new DialogProjectReplicaVariantSavedState()
            {
                LanguageId = Language?.ProjectId.ToString(),
                Text = Text?.Trim() ?? string.Empty,
                VoiceId = Voice?.ProjectId.ToString()
            };
        }

        public override string ToString()
        {
            return Text;
        }

        #endregion
    }
}
