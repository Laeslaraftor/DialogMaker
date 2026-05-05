using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReferenceSavedState : JsonData
    {
        [JsonProperty("path")]
        public string? ItemPath { get; set; }
    }
}
