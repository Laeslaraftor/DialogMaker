using System;

namespace DialogMaker.Core
{
    public class DialogProjectReplica : ObservableObject
    {
        public DialogProjectReplica(DialogProjectResources resources)
        {
            Resources = resources;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public DialogProjectResources Resources { get; }
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
        public Guid? VoiceId
        {
            get => _voiceId;
            set
            {
                if (_voiceId != value)
                {
                    _voiceId = value;
                    InvokePropertyChanged(nameof(VoiceId));
                }
            }
        }

        private string _text = string.Empty;
        private Guid? _voiceId;

        #region Управление

        public DialogProjectResourceItem? GetVoiceResource()
        {
            if (VoiceId == null)
            {
                return null;
            }

            Guid idValue = VoiceId.Value;

            foreach (var item in Resources.Items)
            {
                if (item.Id == idValue)
                {
                    return item;
                }
            }

            return null;    
        }

        #endregion
    }
}
