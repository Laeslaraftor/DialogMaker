using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class LiteralInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpLiteralValue value)
            : Instruction(builder, operation)
        {
            public DSharpLiteralValue Value { get; set; } = value;
            public override int SizeInBytes
            {
                get
                {
                    int size = base.SizeInBytes + 1;

                    if (DSharpBuildInTypes.TryGetTypeInfo(Value.Type, out var info))
                    {
                        size += info.Size;
                    }

                    return size;
                }
            }

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);
                Value.Write(stream);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new LiteralInstruction(builder, Operation, Value);
            }
            public override object[] GetArguments()
            {
                return [Value];
            }

            public override string ToString()
            {
                if (Value.IsString)
                {
                    return $"{Operation} \"{Value}\"";
                }
                if (Value.IsChar)
                {
                    return $"{Operation} '{Value}'";
                }

                return $"{Operation} {Value}";
            }

            #endregion
        }
    }
}
