using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectNodePortSavedState : JsonData, IIdReplaceable
    {
        [JsonProperty("connections")]
        public Dictionary<Guid, List<int>> Connections { get; set; } = [];
        [JsonProperty("name")]
        public object? Value { get; set; }

        #region Управление

        public bool ContainsId(Guid id)
        {
            return Connections.ContainsKey(id);
        }
        public void ReplaceIds(IDictionary<Guid, Guid> identifiersMap)
        {
            Dictionary<Guid, List<int>> connections = new(Connections.Count);

            foreach (var info in Connections)
            {
                if (!identifiersMap.TryGetValue(info.Key, out var id))
                {
                    id = info.Key;
                }

                if (!connections.TryAdd(id, info.Value))
                {
                    connections[id].AddRange(info.Value);
                }
            }

            Connections = connections;
        }

        #endregion
    }
}
