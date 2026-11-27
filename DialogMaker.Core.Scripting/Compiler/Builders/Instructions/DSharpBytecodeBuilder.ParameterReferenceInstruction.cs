using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ParameterReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter)
            : ParameterInstruction(builder, operation, parameter), IReferenceInstruction
        {
            public Instruction? ReferencedInstruction { get; set; }
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                ReferenceInstruction.Write(stream, this);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                IDSharpParameterInfo parameter = ReplaceParameter(BytecodeBuilder, builder, Parameter);
                return new ParameterReferenceInstruction(builder, Operation, parameter)
                {
                    ReferencedInstruction = ReferencedInstruction
                };
            }
            public override object[] GetArguments()
            {
                if (ReferencedInstruction == null)
                {
                    return base.GetArguments();
                }

                return [Parameter, ReferencedInstruction];
            }

            public override string ToString()
            {
                return $"{Operation} [{Parameter?.Name}, {ReferenceInstruction.ToStringReference(this)}]";
            }

            #endregion
        }
    }
}
