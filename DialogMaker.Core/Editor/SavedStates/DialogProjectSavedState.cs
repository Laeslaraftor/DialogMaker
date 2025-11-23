using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("defaultLanguage")]
        public string? DefaultLanguage { get; set; }
        [JsonProperty("packs")]
        public string[] Packs { get; set; } = Array.Empty<string>();
        [JsonProperty("languages")]
        public DialogProjectLanguageSavedState[] Languages { get; set; } = Array.Empty<DialogProjectLanguageSavedState>();
    }
}
