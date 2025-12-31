using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourceCharacterSavedState : DialogResourceObjectSavedState
    {
        [Key(1)]
        public string? Name { get; set; }
    }
}
