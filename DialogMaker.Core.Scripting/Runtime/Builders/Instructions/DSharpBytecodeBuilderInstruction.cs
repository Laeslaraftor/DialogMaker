namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class Instruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
        {
            public DSharpBytecodeOperation Operation { get; } = operation;
            public DSharpBytecodeBuilder BytecodeBuilder { get; } = builder;

            #region Управление

            /// <summary>
            /// Write instruction to stream
            /// </summary>
            /// <param name="stream">Stream for writing instruction</param>
            public virtual void Write(Stream stream)
            {
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return Operation.ToString();
            }

            #endregion
        }
        public class IndexInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, int index) 
            : Instruction(builder, operation)
        {
            public int Index { get; set; } = index;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} {Index}";
            }

            #endregion
        }
        public class OffsetCountInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, int offset, int count) 
            : Instruction(builder, operation)
        {
            public int Offset { get; set; } = offset;
            public int Count { get; set; } = count;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} {Offset} {Count}";
            }

            #endregion
        }
        public class ParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpMethodBuilderParameter parameter) 
            : Instruction(builder, operation)
        {
            public DSharpMethodBuilderParameter Parameter { get; set; } = parameter;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} [{Parameter?.Name}]";
            }

            #endregion
        }
        public class TypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo) 
            : Instruction(builder, operation)
        {
            public TypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpTypeToken memberToken)
                : this(builder, operation, builder.Method.Assembly.GetType(memberToken))
            {
            }

            public IDSharpMemberInfo MemberInfo { get; set; } = memberInfo;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} [{MemberInfo}]";
            }

            #endregion
        }
        public class SizedTypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, int size) 
            : TypeInstruction(builder, operation, memberInfo)
        {
            public int Size { get; set; } = size;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} {Size}";
            }

            #endregion
        }
        public class LiteralInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpLiteralValue value)
            : Instruction(builder, operation)
        {
            public DSharpLiteralValue Value { get; set; } = value;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} {Value}";
            }

            #endregion
        }
        public class ReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation) 
            : Instruction(builder, operation)
        {
            public Instruction? ReferencedInstruction { get; set; }

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} [{ReferencedInstruction}]";
            }

            #endregion
        }
    }
}
