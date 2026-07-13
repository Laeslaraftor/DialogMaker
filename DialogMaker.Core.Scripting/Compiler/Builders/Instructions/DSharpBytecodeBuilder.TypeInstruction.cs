using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo)
            : Instruction(builder, operation)
        {
            public TypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpTypeToken memberToken)
                : this(builder, operation, builder.Method.Assembly.GetType(memberToken))
            {
            }

            public IDSharpMemberInfo MemberInfo { get; set; } = memberInfo;
            public unsafe override int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken);

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var token = BytecodeBuilder.Method.Assembly.GetTypeToken(MemberInfo);
                ((DSharpMetadataToken)token).Write(stream);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new TypeInstruction(builder, Operation, MemberInfo);
            }
            public override object[] GetArguments()
            {
                return [MemberInfo];
            }

            public override string ToString()
            {
                return $"{Operation} [{MemberInfo}]";
            }

            #endregion
        }
    }
}
