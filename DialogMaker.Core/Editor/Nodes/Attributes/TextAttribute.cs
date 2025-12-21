using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public sealed class TextAttribute : Attribute
    {
        public bool AllowMultiline { get; set; }
    }
}
