using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TypedReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpType type)
            : ReferenceInstruction(builder, operation)
        {
            public IDSharpType Type { get; } = type;
            public unsafe override int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken);

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var token = BytecodeBuilder.Method.Assembly.GetTypeToken(Type);
                ((DSharpMetadataToken)token).Write(stream);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new TypedReferenceInstruction(builder, Operation, Type)
                {
                    ReferencedInstruction = ReferencedInstruction
                };
            }
            public override object[] GetArguments()
            {
                if (ReferencedInstruction == null)
                {
                    return [Type];
                }

                return [ReferencedInstruction, Type];
            }

            public override string ToString()
            {
                if (ReferencedInstruction == null)
                {
                    return $"{Operation} [{Type}]";
                }

                int index = BytecodeBuilder.Instructions.IndexOf(ReferencedInstruction);

                return $"{Operation} [{index}: {ReferencedInstruction}, {Type}]";
            }

            #endregion
        }
    }
}
