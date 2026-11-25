using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class BoolArgTypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, bool value) 
            : TypeInstruction(builder, operation, memberInfo)
        {
            /// <summary>
            /// Argument value
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
                return new BoolArgTypeInstruction(builder, Operation, MemberInfo, Value);
            }
            public override object[] GetArguments()
            {
                return [MemberInfo, Value];
            }

            public override string ToString()
            {
                return $"{Operation} [{MemberInfo}, {Value}]";
            }

            #endregion
        }
    }
}
