using DialogMaker.Core.Scripting.Runtime;
namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ParameterMemberInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter, IDSharpMemberInfo memberInfo)
            : ParameterInstruction(builder, operation, parameter)
        {
            public IDSharpMemberInfo MemberInfo { get; set; } = memberInfo;
            public override unsafe int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);

                var token = BytecodeBuilder.Method.Assembly.GetTypeToken(MemberInfo);
                ((DSharpMetadataToken)token).Write(stream);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                var parameter = ReplaceParameter(BytecodeBuilder, builder, Parameter);
                return new ParameterMemberInstruction(builder, Operation, parameter, MemberInfo);
            }
            public override object[] GetArguments()
            {
                return [Parameter, MemberInfo];
            }

            public override string ToString()
            {
                return $"{Operation} [{Parameter?.Name}, {MemberInfo}]";
            }

            #endregion
        }
    }
}
