using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReplicaSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
        [JsonProperty("voiceId")]
        public string? VoiceId { get; set; }
    }
}
