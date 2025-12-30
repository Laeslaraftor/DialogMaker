using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourceStringSavedState : DialogResourceObjectSavedState
    {
        [Key(1)]
        public DialogResourceStringVariantSavedState[] Variants { get; set; } = [];
    }
}
