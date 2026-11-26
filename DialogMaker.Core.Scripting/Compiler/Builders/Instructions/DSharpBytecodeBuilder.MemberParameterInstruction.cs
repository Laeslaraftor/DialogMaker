using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class MemberParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, IDSharpParameterInfo parameter) 
            : TypeInstruction(builder, operation, memberInfo)
        {
            public IDSharpParameterInfo Parameter { get; set; } = parameter;
            public override int SizeInBytes => base.SizeInBytes + sizeof(uint);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);

                int index = BytecodeBuilder.GetVariableIndex(Parameter);

                if (index == -1)
                {
                    throw new InvalidOperationException($"Unable to write local variable that not exists in current method \"{BytecodeBuilder.Method}\". Instruction index: {BytecodeBuilder.Instructions.IndexOf(this)}");
                }

                stream.Write((uint)index);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                var parameter = ParameterInstruction.ReplaceParameter(BytecodeBuilder, builder, Parameter);
                return new MemberParameterInstruction(builder, Operation, MemberInfo, parameter);
            }
            public override object[] GetArguments()
            {
                return [MemberInfo, Parameter];
            }

            public override string ToString()
            {
                return $"{Operation} [{MemberInfo}, {Parameter?.Name}]";
            }

            #endregion
        }
    }
}
