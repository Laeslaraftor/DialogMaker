using System;

namespace DialogMaker.Core.Executioning
{
    public sealed class ArgsCountAttribute(uint argumentsCount) : Attribute
    {
        public uint ArgumentsCount { get; } = argumentsCount;
    }
}
