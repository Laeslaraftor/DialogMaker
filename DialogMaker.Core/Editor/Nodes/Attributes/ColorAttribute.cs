using System;
using System.Drawing;

namespace DialogMaker.Core.Editor.Nodes
{
    public sealed class ColorAttribute(byte r, byte g, byte b) : Attribute
    {
        public byte R { get; } = r;
        public byte G { get; } = g;
        public byte B { get; } = b;
        public Color Color => Color.FromArgb(R, G, B);
    }
}
