using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ParameterMemberReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter, IDSharpMemberInfo memberInfo)
            : ParameterMemberInstruction(builder, operation, parameter, memberInfo), IReferenceInstruction
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
                return new ParameterMemberReferenceInstruction(builder, Operation, parameter, MemberInfo)
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

                return [Parameter, MemberInfo, ReferencedInstruction];
            }

            public override string ToString()
            {
                return $"{Operation} [{Parameter?.Name}, {MemberInfo}, {ReferenceInstruction.ToStringReference(this)}]";
            }

            #endregion
        }
    }
}
