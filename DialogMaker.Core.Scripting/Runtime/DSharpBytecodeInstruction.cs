namespace DialogMaker.Core.Scripting.Runtime
{
    public readonly struct DSharpBytecodeInstruction(DSharpBytecodeOperation operation)
    {
        public DSharpBytecodeInstruction(DSharpBytecodeOperation operation, int index)
            : this(operation)
        {
            Index = index;
        }
        public DSharpBytecodeInstruction(DSharpBytecodeOperation operation, DSharpMetadataToken metadataToken)
            : this(operation)
        {
            MetadataToken = metadataToken;
        }
        public DSharpBytecodeInstruction(DSharpBytecodeOperation operation, DSharpLiteralValue literalValue)
            : this(operation)
        {
            LiteralValue = literalValue;
        }

        public DSharpBytecodeOperation Operation { get; } = operation;
        public int Index { get; }
        public DSharpMetadataToken? MetadataToken { get; }
        public DSharpLiteralValue? LiteralValue { get; }
    }
}
