using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourceItemSavedState : DialogProjectResourceObjectSavedState
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("type")]
        public DialogResourceType ResourceType { get; set; }
    }
}
