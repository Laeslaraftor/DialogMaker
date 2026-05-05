using DialogMaker.Core.Executioning.Builders;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogCompilerOutput(DialogMetadata metadata, byte[] byteCode, DialogExecutionContextBuilder context)
    {
        public DialogMetadata Metadata { get; } = metadata;
        public byte[] Code { get; } = byteCode;
        public DialogExecutionContextBuilder Context { get; } = context;

        #region Управление

        public void Write(Stream stream)
        {
            Metadata.Write(stream);
            stream.Write(Code);
        }
        public MemoryStream Write()
        {
            MemoryStream result = new();
            Write(result);

            result.Position = 0;

            return result;
        }

        #endregion
    }
}
