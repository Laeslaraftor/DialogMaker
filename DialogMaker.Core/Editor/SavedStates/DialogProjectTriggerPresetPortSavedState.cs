using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectTriggerPresetPortSavedState : JsonData
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("value")]
        public object? Value { get; set; }
        [JsonProperty("valueType")]
        public AllowedObjectValues ValueType { get; set; }
    }
}
