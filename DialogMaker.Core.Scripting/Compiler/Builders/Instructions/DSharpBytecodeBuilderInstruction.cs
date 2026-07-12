using DialogMaker.Core.Scripting.Runtime;
using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class Instruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
        {
            public DSharpBytecodeOperation Operation { get; } = operation;
            public DSharpBytecodeBuilder BytecodeBuilder { get; } = builder;
            public virtual int SizeInBytes => sizeof(DSharpBytecodeOperation);

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
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

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
            public override int SizeInBytes => base.SizeInBytes + sizeof(int) * 2;

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
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

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
            public unsafe override int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken);

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
        public class GenericCallingInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, DSharpMethodCallingInfo callingInfo)
            : Instruction(builder, operation)
        {
            public DSharpMethodCallingInfo CallingInfo { get; set; } = callingInfo;
            public unsafe override int SizeInBytes => base.SizeInBytes + (sizeof(DSharpMetadataToken) * (CallingInfo.GenericParameters.Count + 1));

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new GenericCallingInstruction(builder, Operation, CallingInfo);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                object[] arguments = new object[CallingInfo.GenericParameters.Count + 1];
                arguments[0] = CallingInfo.Method;

                int i = 1;

                foreach (var info in CallingInfo.GenericParameters)
                {
                    arguments[i] = info.Value;
                    i++;
                }

                return arguments;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} [{CallingInfo.Method.ToString(CallingInfo.GenericParameters)}]";
            }

            #endregion
        }

        public class SizedTypeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpMemberInfo memberInfo, int size) 
            : TypeInstruction(builder, operation, memberInfo)
        {
            public int Size { get; set; } = size;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

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
            public override int SizeInBytes
            {
                get
                {
                    int size = base.SizeInBytes + 1;

                    if (DSharpBuildInTypes.TryGetTypeInfo(Value.Type, out var info))
                    {
                        size += info.Size;
                    }

                    return size;
                }
            }

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
        public class SizeInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpType type)
            : Instruction(builder, operation)
        {
            public IDSharpType Type { get; } = type;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new SizeInstruction(builder, Operation, Type);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Type.Size];
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Operation} {Type.Size}";
            }

            #endregion
        }
        public class IndexLiteralInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, int index, DSharpLiteralValue value)
            : LiteralInstruction(builder, operation, value)
        {
            public int Index { get; set; } = index;
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new IndexLiteralInstruction(builder, Operation, Index, Value);
            }
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override object[] GetArguments()
            {
                return [Index, Value];
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                return $"{Index} {base.ToString()}";
            }

            #endregion
        }
        public class ReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation) 
            : Instruction(builder, operation)
        {
            public Instruction? ReferencedInstruction { get; set; }
            public override int SizeInBytes => base.SizeInBytes + sizeof(int);

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
        public class TypedReferenceInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, IDSharpType type)
            : ReferenceInstruction(builder, operation)
        {
            public IDSharpType Type { get; } = type;
            public unsafe override int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken);

            #region Управление

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="builder"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new TypedReferenceInstruction(builder, Operation, Type)
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
                    return [Type];
                }

                return [Type, ReferencedInstruction];
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <returns><inheritdoc/></returns>
            public override string ToString()
            {
                if (ReferencedInstruction == null)
                {
                    return $"{Operation} [{Type}]";
                }

                int index = BytecodeBuilder.Instructions.IndexOf(ReferencedInstruction);

                return $"{Operation} [{Type}, {index}: {ReferencedInstruction}]";
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
