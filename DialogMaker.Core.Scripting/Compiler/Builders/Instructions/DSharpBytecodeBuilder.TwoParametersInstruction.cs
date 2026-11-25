using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TwoParametersInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo firstParameter, IDSharpParameterInfo secondParameter) 
            : Instruction(builder, operation)
        {
            /// <summary>
            /// First instruction variable
            /// </summary>
            public IDSharpParameterInfo FirstParameter { get; set; } = firstParameter;
            /// <summary>
            /// Second instruction variable
            /// </summary>
            public IDSharpParameterInfo SecondParameter { get; set; } = secondParameter;
            public override int SizeInBytes => base.SizeInBytes + sizeof(uint) * 2;

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);

                int firstVariableIndex = BytecodeBuilder.GetVariableIndex(FirstParameter);
                int secondVariableIndex = BytecodeBuilder.GetVariableIndex(SecondParameter);

                if (firstVariableIndex == -1 ||
                    secondVariableIndex == -1)
                {
                    throw new InvalidOperationException($"Unable to write local variable that not exists in current method \"{BytecodeBuilder.Method}\". Instruction index: {BytecodeBuilder.Instructions.IndexOf(this)}");
                }

                stream.Write((uint)firstVariableIndex);
                stream.Write((uint)secondVariableIndex);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                var firstParameter = ParameterInstruction.ReplaceParameter(BytecodeBuilder, builder, FirstParameter);
                var secondParameter = ParameterInstruction.ReplaceParameter(BytecodeBuilder, builder, SecondParameter);

                return new TwoParametersInstruction(builder, Operation, firstParameter, secondParameter);
            }
            public override object[] GetArguments()
            {
                return [FirstParameter, SecondParameter];
            }

            public override string ToString()
            {
                return $"{Operation} [{FirstParameter?.Name}, {SecondParameter?.Name}]";
            }

            #endregion
        }
    }
}
