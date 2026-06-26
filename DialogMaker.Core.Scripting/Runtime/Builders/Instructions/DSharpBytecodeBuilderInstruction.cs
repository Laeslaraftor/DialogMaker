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
            /// Create copy of current instruction for other bytecode builder
            /// </summary>
            /// <param name="builder">New bytecode builder</param>
            /// <returns>Copy of current instruction</returns>
            public virtual Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new(builder, Operation);
            }
            /// <summary>
            /// Get array of arguments of this instruction
            /// </summary>
            /// <returns>Array of arguments</returns>
            public virtual object[] GetArguments()
            {
                return [];
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
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new IndexInstruction(builder, Operation, Index);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Index];
            }

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
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new OffsetCountInstruction(builder, Operation, Offset, Count);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Offset, Count];
            }

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
        public class ParameterInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpParameterInfo parameter) 
            : Instruction(builder, operation)
        {
            public IDSharpParameterInfo Parameter { get; set; } = parameter;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new ParameterInstruction(builder, Operation, Parameter);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Parameter];
            }

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
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new TypeInstruction(builder, Operation, MemberInfo);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [MemberInfo];
            }

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
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new SizedTypeInstruction(builder, Operation, MemberInfo, Size);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Size];
            }

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
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new LiteralInstruction(builder, Operation, Value);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Value];
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                if (Value.IsString)
                {
                    return $"{Operation} \"{Value}\"";
                }
                if (Value.IsChar)
                {
                    return $"{Operation} '{Value}'";
                }

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
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new ReferenceInstruction(builder, Operation)
                {
                    ReferencedInstruction = ReferencedInstruction
                };
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                if (ReferencedInstruction == null)
                {
                    return [];
                }

                return [ReferencedInstruction];
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                if (ReferencedInstruction == null)
                {
                    return base.ToString();
                }

                int index = BytecodeBuilder.Instructions.IndexOf(ReferencedInstruction);

                return $"{Operation} [{index}: {ReferencedInstruction}]";
            }

            #endregion
        }
        public class CommentInstruction(DSharpBytecodeBuilder builder, string? text = null) : Instruction(builder, DSharpBytecodeOperation.Empty)
        {
            public string? Text { get; set; } = text;

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new CommentInstruction(builder, Text);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [];
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return string.Empty;
                }

                return $"; {Text}";
            }

            #endregion
        }
    }
}
