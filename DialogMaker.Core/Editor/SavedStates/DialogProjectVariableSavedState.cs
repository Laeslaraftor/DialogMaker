using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectVariableSavedState : DialogProjectResourceObjectSavedState
    {
        [JsonProperty("type")]
        public DialogVariableType Type { get; set; }
        [JsonProperty("value")]
        public object? Value { get; set; }
    }
}
