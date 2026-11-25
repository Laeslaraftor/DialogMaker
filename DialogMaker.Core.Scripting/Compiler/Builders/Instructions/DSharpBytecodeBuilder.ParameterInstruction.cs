using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter)
            : Instruction(builder, operation)
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
                IDSharpParameterInfo parameter = ReplaceParameter(BytecodeBuilder, builder, Parameter);
                return new ParameterInstruction(builder, Operation, parameter);
            }
            public override object[] GetArguments()
            {
                return [Parameter];
            }

            public override string ToString()
            {
                return $"{Operation} [{Parameter?.Name}]";
            }

            #endregion

            #region Static

            internal static IDSharpParameterInfo ReplaceParameter(DSharpBytecodeBuilder currentBuilder, DSharpBytecodeBuilder newBuilder, IDSharpParameterInfo parameter)
            {
                int index = currentBuilder.Method.Parameters.IndexOf(parameter);

                if (index != -1)
                {
                    if (newBuilder.Method.Parameters.Count > index)
                    {
                        parameter = newBuilder.Method.Parameters[index];
                    }
                }
                else
                {
                    index = currentBuilder.LocalVariables.IndexOf(parameter);

                    if (newBuilder.LocalVariables.Count > index)
                    {
                        parameter = newBuilder.LocalVariables[index];
                    }
                }

                return parameter;
            }

            #endregion
        }
    }
}
