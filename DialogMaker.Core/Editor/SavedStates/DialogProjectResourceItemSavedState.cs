using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourceItemSavedState
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("filePath")]
        public string FilePath { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("type")]
        public DialogResourceType ResourceType { get; set; }
    }
}
