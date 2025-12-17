using DialogMaker.Core.Editor.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        [JsonProperty("inputs")]
        public Dictionary<int, DialogProjectNodePortSavedState> Inputs { get; set; } = [];
        [JsonProperty("outputs")]
        public Dictionary<int, DialogProjectNodePortSavedState> Outputs { get; set; } = [];
        [JsonProperty("properties")]
        public Dictionary<string, object?> Properties { get; set; } = [];

        #region Управление

        public T? GetProperty<T>(string propertyName)
        {
            if (!Properties.TryGetValue(propertyName, out var value) ||
                value is not JToken token)
            {
                return default;
            }

            return token.ToObject<T>();
        }
        public DialogProjectReference<T>? RestoreReference<T>(DialogProject project, string name) 
            where T : DialogProjectResourceObject
        {
            var savedState = GetProperty<DialogProjectReferenceSavedState>(name);

            if (savedState == null)
            {
                return null;
            }

            try
            {
                return DialogProjectReference<T>.Restore(project, savedState);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }

            return null;
        }

        #endregion
    }
}
