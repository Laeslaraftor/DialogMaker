using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
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
    }
}
