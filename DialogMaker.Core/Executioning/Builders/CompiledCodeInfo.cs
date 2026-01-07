namespace DialogMaker.Core.Executioning.Builders
{
    public readonly struct CompiledCodeInfo(byte[] code, DialogExecutionContextBuilder context)
    {
        public byte[] ByteCode { get; } = code;
        public DialogExecutionContextBuilder Context { get; } = context;
    }
}
