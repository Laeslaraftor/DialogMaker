using System;

namespace DialogMaker.Core
{
    public class DialogProjectReplica
    {
        public DialogProjectReplica(DialogProjectResources resources)
        {
            Resources = resources;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public DialogProjectResources Resources { get; }
        public string Text { get; set; } = string.Empty;
        public Guid? VoiceId { get; set; }
    }
}
