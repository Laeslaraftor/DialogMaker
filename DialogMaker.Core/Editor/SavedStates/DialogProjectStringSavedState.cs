using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectStringSavedState : JsonData
    {
        [JsonProperty("projectId")]
        public string ProjectId { get; set; } = string.Empty;
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("text")]
        public DialogProjectStringVariantSavedState[] Variants { get; set; } = Array.Empty<DialogProjectStringVariantSavedState>();
    }
}
