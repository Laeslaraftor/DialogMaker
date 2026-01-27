using System;

namespace DialogMaker.Core
{
    public sealed class RangeAttribute(float minimum, float maximum) : Attribute
    {
        public float Minimum { get; } = minimum;
        public float Maximum { get; } = maximum;
    }
}
