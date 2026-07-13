using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class IndexLiteralInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, int index, DSharpLiteralValue value)
            : LiteralInstruction(builder, operation, value)
        {
            public int Index { get; set; } = index;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var indexBytes = BitConverter.GetBytes(Index);
                stream.Write(indexBytes);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new IndexLiteralInstruction(builder, Operation, Index, Value);
            }
            public override object[] GetArguments()
            {
                return [Value, Index];
            }

            public override string ToString()
            {
                return $"{base.ToString()}, {Index}";
            }

            #endregion
        }
    }
}
