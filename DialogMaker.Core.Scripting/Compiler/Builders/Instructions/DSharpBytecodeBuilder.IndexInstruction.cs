using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
	public partial class DSharpBytecodeBuilder
	{
		public class IndexInstruction(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, uint index)
			: Instruction(builder, operation)
		{
			public uint Index { get; set; } = index;
			public override int SizeInBytes => base.SizeInBytes + sizeof(uint);

            #region Управление

            public override void Write(Stream stream)
            {
                base.Write(stream);
				stream.Write(Index);
            }
			public override Instruction Copy(DSharpBytecodeBuilder builder)
			{
				return new IndexInstruction(builder, Operation, Index);
			}
			public override object[] GetArguments()
			{
				return [Index];
			}

			public override string ToString()
			{
				return $"{Operation} {Index}";
			}

			#endregion
		}
	}
}
