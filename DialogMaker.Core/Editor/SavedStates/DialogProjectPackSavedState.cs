using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectPackSavedState
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("dialogs")]
        public string[] Dialogs { get; set; } = Array.Empty<string>();
    }
}
