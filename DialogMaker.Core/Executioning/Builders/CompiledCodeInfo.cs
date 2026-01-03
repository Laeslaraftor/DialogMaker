using System.Collections.Generic;

namespace DialogMaker.Core.Executioning.Builders
{
    public readonly struct CompiledCodeInfo(byte[] code, DialogExecutionContextBuilder context, Dictionary<int, int> sections)
    {
        public byte[] ByteCode { get; } = code;
        public DialogExecutionContextBuilder Context { get; } = context;
        public Dictionary<int, int> SectionPosition { get; } = sections;
    }
}
