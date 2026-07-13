using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class SizeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpType type)
            : Instruction(builder, operation)
        {
            public IDSharpType Type { get; } = type;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var size = Type.GetSize(true, true);

                if (size == -1)
                {
                    throw new InvalidOperationException($"Unable to write size of \"{Type}\" because it depends on execution platform");
                }

                var sizeBytes = BitConverter.GetBytes(size);
                stream.Write(sizeBytes);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new SizeInstruction(builder, Operation, Type);
            }
            public override object[] GetArguments()
            {
                return [Type.GetSize(true, true)];
            }

            public override string ToString()
            {
                return $"{Operation} {Type.GetSize(true, true)}";
            }

            #endregion
        }
    }
}
