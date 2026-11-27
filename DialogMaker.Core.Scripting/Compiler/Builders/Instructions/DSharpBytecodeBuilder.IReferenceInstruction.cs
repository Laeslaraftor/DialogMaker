namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public partial class DSharpBytecodeBuilder
    {
        /// <summary>
        /// Interface of instruction that references to other
        /// </summary>
        public interface IReferenceInstruction
        {
            public Instruction? ReferencedInstruction { get; set; }
        }
    }
}
