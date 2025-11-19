using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectSavedState
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("packs")]
        public string[] Packs { get; set; } = Array.Empty<string>();
    }
}
