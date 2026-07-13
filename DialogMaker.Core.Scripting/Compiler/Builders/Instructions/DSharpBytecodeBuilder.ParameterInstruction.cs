using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class ParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter)
            : Instruction(builder, operation)
        {
            public IDSharpParameterInfo Parameter { get; set; } = parameter;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

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

                var indexBytes = BitConverter.GetBytes(index);
                stream.Write(indexBytes);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new ParameterInstruction(builder, Operation, Parameter);
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
