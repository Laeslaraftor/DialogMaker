using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectCharacterSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
