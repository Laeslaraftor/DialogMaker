using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourceFileSavedState : DialogResourceObjectSavedState
    {
        [Key(1)]
        public string FileName { get; set; } = string.Empty;
        [Key(2)]
        public DialogFileResourceType Type { get; set; }
    }
}
