using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourcesSavedState
    {
        [Key(0)]
        public Dictionary<string, DialogResourceFileSavedState> Files { get; set; } = [];
        [Key(1)]
        public Dictionary<string, DialogResourceStringSavedState> Strings { get; set; } = [];
        [Key(2)]
        public Dictionary<string, DialogResourceCharacterSavedState> Characters { get; set; } = [];
        [Key(3)]
        public Dictionary<string, DialogResourceVariableSavedState> Variables { get; set; } = [];
        [Key(4)]
        public Dictionary<string, DialogResourceEmotionSavedState> Emotions { get; set; } = [];
    }
}
