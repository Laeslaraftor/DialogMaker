using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectStringVariantSavedState : JsonData
    {
        [JsonProperty("language")]
        public string? LanguageId { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
        [JsonProperty("voice")]
        public DialogProjectReferenceSavedState? VoiceId { get; set; }
    }
}
