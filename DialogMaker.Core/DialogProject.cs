using System;
using System.Collections.Generic;

namespace DialogMaker.Core
{
    public class DialogProject
    {
        public string Name { get; set; } = string.Empty;
        public List<DialogProjectPack> Packs { get; set; } = new();
    }
}
