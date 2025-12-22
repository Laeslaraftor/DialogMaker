using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourceItemSavedState : DialogProjectResourceObjectSavedState
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;
        [JsonProperty("type")]
        public DialogFileResourceType ResourceType { get; set; }
    }
}
