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

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);

                int index = BytecodeBuilder.Method.Parameters.IndexOf(Parameter);

                if (index == -1)
                {
                    index = BytecodeBuilder.LocalVariables.IndexOf(Parameter);

                    if (index == -1)
                    {
                        throw new InvalidOperationException($"Unable to write local variable that not exists in current method \"{BytecodeBuilder.Method}\"");
                    }
                    else
                    {
                        index += BytecodeBuilder.Method.Parameters.Count;
                    }
                }

                stream.Write((uint)index);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                int index = BytecodeBuilder.Method.Parameters.IndexOf(Parameter);
                IDSharpParameterInfo parameter = Parameter; 

                if (index != -1)
                {
                    if (builder.Method.Parameters.Count > index)
                    {
                        parameter = builder.Method.Parameters[index];
                    }
                }
                else
                {
                    index = BytecodeBuilder.LocalVariables.IndexOf(Parameter);

                    if (builder.LocalVariables.Count > index)
                    {
                        parameter = builder.LocalVariables[index];
                    }
                }

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
        }
    }
}
