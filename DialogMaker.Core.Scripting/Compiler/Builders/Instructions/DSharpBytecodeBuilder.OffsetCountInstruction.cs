using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class OffsetCountInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, int offset, int count)
            : Instruction(builder, operation)
        {
            public int Offset { get; set; } = offset;
            public int Count { get; set; } = count;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int) * 2;

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var offsetBytes = BitConverter.GetBytes(Offset);
                var countBytes = BitConverter.GetBytes(Count);

                stream.Write(offsetBytes);
                stream.Write(countBytes);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new OffsetCountInstruction(builder, Operation, Offset, Count);
            }
            public override object[] GetArguments()
            {
                return [Offset, Count];
            }

            public override string ToString()
            {
                return $"{Operation} {Offset} {Count}";
            }

            #endregion
        }
    }
}
