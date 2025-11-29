using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectStringSavedState : DialogProjectResourceObjectSavedState
    {
        [JsonProperty("text")]
        public DialogProjectStringVariantSavedState[] Variants { get; set; } = Array.Empty<DialogProjectStringVariantSavedState>();
    }
}
