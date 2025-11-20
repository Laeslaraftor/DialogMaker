using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReplica : ObservableObject, ISavable
    {
        public DialogProjectReplica(DialogProjectResources resources)
        {
            Resources = resources;
            Id = Guid.NewGuid();
        }
        public DialogProjectReplica(DialogProjectResources resources, DialogProjectReplicaSavedState savedState)
        {
            Resources = resources;
            Id = Guid.Parse(savedState.Id);
            Text = savedState.Text;
            
            if (savedState.VoiceId != null && 
                Guid.TryParse(savedState.VoiceId, out var voiceId) &&
                Resources.TryGetItem(voiceId, out var voice))
            {
                Voice = voice;
            }
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
            get => _voice?.Id;
            set
            {
                if (_voiceId != value)
                {
                    if (value != null && _voice != null && _voice.Id != value)
                    {
                        if (Resources.TryGetItem(value.Value, out var item))
                        {
                            _voiceId = value;
                            Voice = item;
                        }
                        else
                        {
                            throw new ArgumentException($"Ресурс с идентификатором {value} не найден", nameof(value));
                        }
                    }
                    else
                    {
                        _voiceId = value;
                    }

                    InvokePropertyChanged(nameof(VoiceId));
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
                    var id = value?.Id;

                    if (_voiceId != id)
                    {
                        VoiceId = id;
                    }

                    InvokePropertyChanged(nameof(Voice));
                }
            }
        }

        private string _text = string.Empty;
        private Guid? _voiceId;
        public DialogProjectResourceItem? _voice;

        #region Управление

        public ISavedState Save()
        {
            return new DialogProjectReplicaSavedState
            {
                Id = Id.ToString(),
                Text = Text,
                VoiceId = _voiceId?.ToString()
            };
        }

        #endregion
    }
}
