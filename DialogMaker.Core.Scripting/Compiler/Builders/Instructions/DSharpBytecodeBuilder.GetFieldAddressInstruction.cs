using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public class GetFieldAddressInstruction(DSharpBytecodeBuilder builder, IDSharpFieldInfo field)
            : GetAddressInstruction(builder, DSharpGetAddressMember.Field)
        {
            public IDSharpFieldInfo Field { get; set; } = field;
            public unsafe override int SizeInBytes => base.SizeInBytes + sizeof(DSharpMetadataToken);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                var token = BytecodeBuilder.Method.Assembly.GetTypeToken(Field);
                ((DSharpMetadataToken)token).Write(stream);
            }
            public override Instruction Copy(DSharpBytecodeBuilder builder)
            {
                return new GetFieldAddressInstruction(builder, Field);
            }
            public override object[] GetArguments()
            {
                return [Member, Field];
            }

            public override string ToString()
            {
                return $"{Operation} [{Member}{(Field != null ? ", " + Field.ToString() : string.Empty)}]";
            }

            #endregion
        }
    }
}
