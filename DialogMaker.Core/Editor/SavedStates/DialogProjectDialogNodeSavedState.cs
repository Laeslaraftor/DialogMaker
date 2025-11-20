using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectDialogNodeSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("nodeType")]
        public DialogNodeType NodeType { get; set; }
    }
}
