using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class TypeSizeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, DSharpLoadTypeSizeMode mode)
            : TypeInstruction(builder, operation, memberInfo)
        {
            /// <summary>
            /// Load size mode
            /// </summary>
            public DSharpLoadTypeSizeMode SizeMode { get; set; } = mode;

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                stream.Write(SizeMode);
            }

            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new TypeSizeInstruction(builder, Operation, MemberInfo, SizeMode);
            }
            public override object[] GetArguments()
            {
                return [MemberInfo, SizeMode];
            }

            public override string ToString()
            {
                return $"{Operation} [{MemberInfo}, {SizeMode}]";
            }

            #endregion
        }
    }
}
