using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourcesSavedState : JsonData
    {
        [JsonProperty("strings")]
        public DialogProjectStringSavedState[] Strings { get; set; } = [];
        [JsonProperty("items")]
        public DialogProjectResourceItemSavedState[] Items { get; set; } = [];
        [JsonProperty("characters")]
        public DialogProjectCharacterSavedState[] Characters { get; set; } = [];
        [JsonProperty("variables")]
        public DialogProjectVariableSavedState[] Variables { get; set; } = [];
        [JsonProperty("emotions")]
        public DialogProjectEmotionSavedState[] Emotions { get; set; } = [];
        [JsonProperty("triggerPresets")]
        public DialogProjectTriggerPresetSavedState[] TriggerPresets { get; set; } = [];
    }
}
