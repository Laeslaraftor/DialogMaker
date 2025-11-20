using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectPackSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("dialogs")]
        public string[] Dialogs { get; set; } = Array.Empty<string>();
        [JsonProperty("resources")]
        public string[] Resources { get; set; } = Array.Empty<string>();
    }
}
