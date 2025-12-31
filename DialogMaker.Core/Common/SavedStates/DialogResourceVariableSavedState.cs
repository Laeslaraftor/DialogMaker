using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourceVariableSavedState : DialogResourceObjectSavedState
    {
        [Key(1)]
        public DialogVariableType Type { get; set; }
        [Key(2)]
        public object? Value { get; set; }
    }
}
