using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class Instruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
        {
            /// <summary>
            /// Operation of this instruction
            /// </summary>
            public DSharpBytecodeOperation Operation { get; } = operation;
            /// <summary>
            /// Bytecode builder that contains current instruction
            /// </summary>
            public DSharpBytecodeBuilder BytecodeBuilder { get; } = builder;
            /// <summary>
            /// Size of instruction in bytes
            /// </summary>
            public virtual int SizeInBytes => sizeof(DSharpBytecodeOperation);

            #region Управление

            /// <summary>
            /// Write instruction to stream
            /// </summary>
            /// <param name="stream">Stream for writing instruction</param>
            public virtual void Write(Stream stream)
            {
                stream.WriteByte((byte)Operation);
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
    }
}
