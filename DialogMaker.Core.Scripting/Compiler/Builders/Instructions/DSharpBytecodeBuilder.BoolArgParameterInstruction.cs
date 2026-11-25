using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class BoolArgParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter, bool value)
            : ParameterInstruction(builder, operation, parameter)
        {
            /// <summary>
            /// Boolean argument value
            /// </summary>
            public bool Value { get; set; } = value;
            public override int SizeInBytes => base.SizeInBytes + sizeof(bool);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                stream.Write(Value);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new BoolArgParameterInstruction(builder, Operation, Parameter, Value);
            }
            public override object[] GetArguments()
            {
                return [Value, Parameter];
            }

            public override string ToString()
            {
                return $"{Operation} [{Value}, {Parameter?.Name}]";
            }

            #endregion
        }
    }
}
