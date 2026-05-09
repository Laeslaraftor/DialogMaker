using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectTriggerPresetSavedState : DialogProjectResourceObjectSavedState
    {
        [JsonProperty("description")]
        public string? Description { get; set; }
        [JsonProperty("inputs")]
        public DialogProjectTriggerPresetPortSavedState[]? Inputs { get; set; }
        [JsonProperty("outputs")]
        public DialogProjectTriggerPresetPortSavedState[]? Outputs { get; set; }
    }
}
