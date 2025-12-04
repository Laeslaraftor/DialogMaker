using Newtonsoft.Json;
using System.Numerics;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialogNodeSavedState : JsonData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("nodeType")]
        public DialogNodeType NodeType { get; set; }
        [JsonProperty("position")]
        public Vector2 Position { get; set; }
    }
}
