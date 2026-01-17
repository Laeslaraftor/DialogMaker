using MessagePack;
using System;
using System.Collections.Generic;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogPackageSavedState
    {
        [Key(0)]
        public string Id { get; set; } = string.Empty;
        [Key(1)]
        public string Name { get; set; } = string.Empty;
        [Key(2)]
        public Dictionary<string, DialogLanguageSavedState> Languages { get; set; } = [];
        [Key(3)]
        public string? CurrentLanguage { get; set; }
        [Key(4)]
        public List<string> Folders { get; set; } = [];
    }
}
