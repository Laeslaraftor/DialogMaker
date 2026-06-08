namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class Instruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
        {
            public DSharpBytecodeOperation Operation { get; } = operation;
            public DSharpBytecodeBuilder BytecodeBuilder { get; } = builder;

            /// <summary>
            /// Write instruction to stream
            /// </summary>
            /// <param name="stream">Stream for writing instruction</param>
            public virtual void Write(Stream stream)
            {
            }
        }
        public class IndexInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, int index) 
            : Instruction(builder, operation)
        {
            public int Index { get; set; } = index;
        }
        public class ParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpMethodBuilderParameter parameter) 
            : Instruction(builder, operation)
        {
            public DSharpMethodBuilderParameter Parameter { get; set; } = parameter;
        }
        public class TypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo) 
            : Instruction(builder, operation)
        {
            public IDSharpMemberInfo MemberInfo { get; set; } = memberInfo;
        }
        public class SizedTypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, int size) 
            : TypeInstruction(builder, operation, memberInfo)
        {
            public int Size { get; set; } = size;
        }
        public class LiteralInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpLiteralValue value)
            : Instruction(builder, operation)
        {
            public DSharpLiteralValue Value { get; set; } = value;
        }
        public class ReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation) 
            : Instruction(builder, operation)
        {
            public Instruction? ReferencedInstruction { get; set; }
        }
    }
}
