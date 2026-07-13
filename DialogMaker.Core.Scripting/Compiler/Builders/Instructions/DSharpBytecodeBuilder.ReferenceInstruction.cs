using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
            : Instruction(builder, operation)
        {
            public Instruction? ReferencedInstruction { get; set; }
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Управление

            public override void Write(Stream stream)
            {
                if (ReferencedInstruction == null)
                {
                    throw new InvalidOperationException("Referenced instruction not specified");
                }

                base.Write(stream);

                int index = BytecodeBuilder.Instructions.IndexOf(ReferencedInstruction);

                if (index == -1)
                {
                    throw new InvalidOperationException($"Referenced instruction ({ReferencedInstruction}) not exists in bytecode at \"{BytecodeBuilder.Method}\"");
                }

                var indexBytes = BitConverter.GetBytes(index);
                stream.Write(indexBytes);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new ReferenceInstruction(builder, Operation)
                {
                    ReferencedInstruction = ReferencedInstruction
                };
            }
            public override object[] GetArguments()
            {
                if (ReferencedInstruction == null)
                {
                    return [];
                }

                return [ReferencedInstruction];
            }

            public override string ToString()
            {
                if (ReferencedInstruction == null)
                {
                    return base.ToString();
                }

                int index = BytecodeBuilder.Instructions.IndexOf(ReferencedInstruction);

                return $"{Operation} [{index}: {ReferencedInstruction}]";
            }

            #endregion
        }
    }
}
