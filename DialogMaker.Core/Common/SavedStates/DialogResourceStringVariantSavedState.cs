using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourceStringVariantSavedState
    {
        [Key(0)]
        public string? Language { get; set; }
        [Key(1)]
        public string Value { get; set; } = string.Empty;
        [Key(2)]
        public string? Voice { get; set; }
    }
}
