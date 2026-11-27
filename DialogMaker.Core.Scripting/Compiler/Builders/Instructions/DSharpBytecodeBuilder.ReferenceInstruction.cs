using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
            : Instruction(builder, operation), IReferenceInstruction
        {
            public Instruction? ReferencedInstruction { get; set; }
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);
                Write(stream, this);
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
                return $"{Operation} [{ToStringReference(this)}]";
            }

            #endregion

            #region Static

            internal static string? ToStringReference<T>(T referenceInstruction)
                where T : Instruction, IReferenceInstruction
            {
                var referencedInstruction = referenceInstruction.ReferencedInstruction;

                if (referencedInstruction == null)
                {
                    return null;
                }

                int index = referenceInstruction.BytecodeBuilder.Instructions.IndexOf(referencedInstruction);
                string reference;

                if (Equals(referencedInstruction, referenceInstruction) ||
                    (referencedInstruction is IReferenceInstruction otherReference &&
                    otherReference.ReferencedInstruction == referenceInstruction))
                {
                    reference = referencedInstruction.Operation.ToString();
                }
                else
                {
                    reference = referencedInstruction.ToString();
                }

                return $"{index}: {reference}";
            }
            internal static void Write<T>(Stream stream, T referenceInstruction)
                where T : Instruction, IReferenceInstruction
            {
                var referencedInstruction = referenceInstruction.ReferencedInstruction;

                if (referencedInstruction == null)
                {
                    throw new InvalidOperationException($"Referenced instruction not specified at \"{referenceInstruction.BytecodeBuilder.Method}\"");
                }

                int index = referenceInstruction.BytecodeBuilder.Instructions.IndexOf(referencedInstruction);

                if (index == -1)
                {
                    throw new InvalidOperationException($"Referenced instruction ({referencedInstruction}) not exists in bytecode at \"{referenceInstruction.BytecodeBuilder.Method}\"");
                }

                stream.Write(index);
            }

            #endregion
        }
    }
}
