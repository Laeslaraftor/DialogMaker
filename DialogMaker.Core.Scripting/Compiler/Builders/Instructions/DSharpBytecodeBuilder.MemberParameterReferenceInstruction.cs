using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class MemberParameterReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, IDSharpParameterInfo parameter)
            : MemberParameterInstruction(builder, operation, memberInfo, parameter), IReferenceInstruction
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
                IDSharpParameterInfo parameter = ParameterInstruction.ReplaceParameter(BytecodeBuilder, builder, Parameter);
                return new MemberParameterReferenceInstruction(builder, Operation, MemberInfo, parameter)
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

                return [MemberInfo, Parameter, ReferencedInstruction];
            }

            public override string ToString()
            {
                return $"{Operation} [{MemberInfo}, {Parameter?.Name}, {ReferenceInstruction.ToStringReference(this)}]";
            }

            #endregion
        }
    }
}
