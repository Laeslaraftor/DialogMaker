using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourcesSavedState : JsonData
    {
        [JsonProperty("replicas")]
        public DialogProjectReplicaSavedState[] Replicas { get; set; } = Array.Empty<DialogProjectReplicaSavedState>();
        [JsonProperty("items")]
        public DialogProjectResourceItemSavedState[] Items { get; set; } = Array.Empty<DialogProjectResourceItemSavedState>();
    }
}
