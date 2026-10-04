using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class GetVariableAddressInstruction(DSharpBytecodeBuilder builder, IDSharpParameterInfo variable) 
            : GetAddressInstruction(builder, DSharpGetAddressMember.Variable)
        {
            public IDSharpParameterInfo Variable { get; set; } = variable;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);

                int index = BytecodeBuilder.GetVariableIndex(Variable);

                if (index == -1)
                {
                    throw new InvalidOperationException($"Unable to write local variable that not exists in current method \"{BytecodeBuilder.Method}\"");
                }

                stream.Write(index);
            }
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                int index = BytecodeBuilder.Method.Parameters.IndexOf(Variable);
                IDSharpParameterInfo parameter = Variable;

                if (index != -1)
                {
                    if (builder.Method.Parameters.Count > index)
                    {
                        parameter = builder.Method.Parameters[index];
                    }
                }
                else
                {
                    index = BytecodeBuilder.LocalVariables.IndexOf(Variable);

                    if (builder.LocalVariables.Count > index)
                    {
                        parameter = builder.LocalVariables[index];
                    }
                }

                return new GetVariableAddressInstruction(builder, parameter);
            }
            public override object[] GetArguments()
            {
                return [Member, Variable];
            }

            public override string ToString()
            {
                return $"{Operation} [{Member}{(Variable != null ? ", " + Variable.Name : string.Empty)}]";
            }

            #endregion
        }
    }
}
