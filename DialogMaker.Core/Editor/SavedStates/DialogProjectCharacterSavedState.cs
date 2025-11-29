using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectCharacterSavedState : DialogProjectResourceObjectSavedState
    {
        [JsonProperty("name")]
        public DialogProjectReferenceSavedState? Name { get; set; }
    }
}
