using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialogSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("nodes")]
        public DialogProjectDialogNodeSavedState[] Nodes { get; set; } = Array.Empty<DialogProjectDialogNodeSavedState>();
    }
}
