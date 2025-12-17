using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodePortSavedState : JsonData
    {
        [JsonProperty("connections")]
        public Dictionary<Guid, List<int>> Connections { get; set; } = [];
        [JsonProperty("name")]
        public object? Value { get; set; }
    }
}
