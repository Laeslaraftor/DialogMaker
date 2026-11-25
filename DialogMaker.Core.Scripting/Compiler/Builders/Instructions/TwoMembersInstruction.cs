using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TwoMembersInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo firstMember, IDSharpMemberInfo secondMember)
            : Instruction(builder, operation)
        {
            /// <summary>
            /// First argument member
            /// </summary>
            public IDSharpMemberInfo FirstMember { get; set; } = firstMember;
            /// <summary>
            /// Second argument member
            /// </summary>
            public IDSharpMemberInfo SecondMember { get; set; } = secondMember;
            public override unsafe int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken) * 2;

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                ((DSharpMetadataToken)BytecodeBuilder.Method.Assembly.GetTypeToken(FirstMember)).Write(stream);
                ((DSharpMetadataToken)BytecodeBuilder.Method.Assembly.GetTypeToken(SecondMember)).Write(stream);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new TwoMembersInstruction(builder, Operation, FirstMember, SecondMember);
            }
            public override object[] GetArguments()
            {
                return [FirstMember, SecondMember];
            }

            public override string ToString()
            {
                return $"{Operation} [{FirstMember}, {SecondMember}]";
            }

            #endregion
        }
    }
}
