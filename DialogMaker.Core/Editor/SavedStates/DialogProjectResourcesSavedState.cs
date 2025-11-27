using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourcesSavedState : JsonData
    {
        [JsonProperty("strings")]
        public DialogProjectStringSavedState[] Strings { get; set; } = Array.Empty<DialogProjectStringSavedState>();
        [JsonProperty("items")]
        public DialogProjectResourceItemSavedState[] Items { get; set; } = Array.Empty<DialogProjectResourceItemSavedState>();
    }
}
