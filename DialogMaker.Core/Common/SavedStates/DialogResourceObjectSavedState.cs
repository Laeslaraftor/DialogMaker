using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    [Union(0, typeof(DialogResourceFileSavedState))]
    [Union(1, typeof(DialogResourceStringSavedState))]
    [Union(2, typeof(DialogResourceCharacterSavedState))]
    [Union(3, typeof(DialogResourceVariableSavedState))]
    public abstract class DialogResourceObjectSavedState
    {
        [Key(0)]
        public string Id { get; set; } = string.Empty;
    }
}
