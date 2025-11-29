using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourceObjectSavedState : JsonData
    {
        [JsonProperty("projectId")]
        public string ProjectId { get; set; } = string.Empty;
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
    }
}
