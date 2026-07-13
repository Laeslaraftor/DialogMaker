using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class CommentInstruction(DSharpBytecodeBuilder builder, string? text = null) : Instruction(builder, DSharpBytecodeOperation.Empty)
        {
            public string? Text { get; set; } = text;

            #region Управление

            public override void Write(Stream stream)
            {
            }
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new CommentInstruction(builder, Text);
            }
            public override object[] GetArguments()
            {
                return [];
            }

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
