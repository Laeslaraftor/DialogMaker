using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectLanguageSavedState : JsonData
    {
        [JsonProperty("internalId")]
        public string ProjectId { get; set; } = string.Empty;
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
