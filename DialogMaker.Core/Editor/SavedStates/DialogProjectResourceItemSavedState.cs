using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourceItemSavedState : JsonData
    {
        [JsonProperty("projectId")]
        public string ProjectId { get; set; } = string.Empty;
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("type")]
        public DialogResourceType ResourceType { get; set; }
    }
}
