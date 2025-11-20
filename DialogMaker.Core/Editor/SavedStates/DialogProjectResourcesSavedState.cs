using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourcesSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;
        [JsonProperty("replicas")]
        public DialogProjectReplicaSavedState[] Replicas { get; set; } = Array.Empty<DialogProjectReplicaSavedState>();
        [JsonProperty("items")]
        public DialogProjectResourceItemSavedState[] Items { get; set; } = Array.Empty<DialogProjectResourceItemSavedState>();
    }
}
