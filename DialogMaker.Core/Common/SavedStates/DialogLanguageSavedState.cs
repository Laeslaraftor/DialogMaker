using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogLanguageSavedState
    {
        [Key(0)]
        public string Id { get; set; } = string.Empty;
        [Key(1)]
        public string Name { get; set; } = string.Empty;
    }
}
