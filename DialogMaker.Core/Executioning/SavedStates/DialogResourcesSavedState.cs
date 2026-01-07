using MessagePack;
using System.Collections.Generic;

namespace DialogMaker.Core.Executioning.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourcesSavedState()
    {
        public DialogResourcesSavedState(Dictionary<int, DialogItemReference> references) 
            : this()
        {
            foreach (var info in references)
            {
                ResourceReferences.Add(info.Key, info.Value.ToString());
            }
        }

        [Key(0)]
        public Dictionary<int, string> ResourceReferences { get; set; } = [];
    }
}
