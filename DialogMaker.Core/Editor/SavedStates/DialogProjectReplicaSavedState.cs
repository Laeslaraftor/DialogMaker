using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReplicaSavedState : JsonData
    {
        [JsonProperty("projectId")]
        public string ProjectId { get; set; } = string.Empty;
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("text")]
        public DialogProjectReplicaVariantSavedState[] Variants { get; set; } = Array.Empty<DialogProjectReplicaVariantSavedState>();
    }
}
