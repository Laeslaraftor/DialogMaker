using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        public abstract class GetAddressInstruction(DSharpBytecodeBuilder builder, DSharpGetAddressMember member) 
            : Instruction(builder, DSharpBytecodeOperation.GetAddress)
        {
            public DSharpGetAddressMember Member { get; } = member;
            public override int SizeInBytes => base.SizeInBytes + sizeof(DSharpGetAddressMember);

            #region Controls

            public override void Write(Stream stream)
            {
                base.Write(stream);
                stream.Write(Member);
            }

            #endregion
        }
    }
}
