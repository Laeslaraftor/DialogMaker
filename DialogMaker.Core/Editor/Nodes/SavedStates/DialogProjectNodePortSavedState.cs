using Newtonsoft.Json;
using System.Collections.Generic;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodePortSavedState : JsonData
    {
        [JsonProperty("connections")]
        public Dictionary<string, int> Connections { get; set; } = [];
    }
}
