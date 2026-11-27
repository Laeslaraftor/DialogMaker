using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TwoMembersReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo firstMember, IDSharpMemberInfo secondMember) 
            : TwoMembersInstruction(builder, operation, firstMember, secondMember), IReferenceInstruction
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
                return new TwoMembersReferenceInstruction(builder, Operation, FirstMember, SecondMember)
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

                return [FirstMember, SecondMember, ReferencedInstruction];
            }

            public override string ToString()
            {
                return $"{Operation} [{FirstMember}, {SecondMember}, {ReferenceInstruction.ToStringReference(this)}]";
            }

            #endregion
        }
    }
}
