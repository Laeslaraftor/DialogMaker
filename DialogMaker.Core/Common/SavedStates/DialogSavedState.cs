using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogSavedState
    {
        [Key(0)]
        public string Id { get; set; } = string.Empty;
        [Key(1)]
        public string Name { get; set; } = string.Empty;
        [Key(2)]
        public byte[] Bytecode { get; set; } = [];
        [Key(3)]
        public Dictionary<int, string> ResourcesIndex { get; set; } = [];
    }
}
