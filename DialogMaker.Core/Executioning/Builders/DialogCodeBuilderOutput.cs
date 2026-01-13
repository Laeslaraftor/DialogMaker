namespace DialogMaker.Core.Executioning.Builders
{
    public readonly struct DialogCodeBuilderOutput(byte[] code, DialogExecutionContextBuilder context)
    {
        public byte[] ByteCode { get; } = code;
        public DialogExecutionContextBuilder Context { get; } = context;
    }
}
