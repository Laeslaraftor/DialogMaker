using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Compiler
{
    internal class TypeSetupRequestEventArgs(DSharpTypeBuilder type) : EventArgs
    {
        public DSharpTypeBuilder Type { get; } = type;
        public bool SetupCompleted { get; set; }
    }
}
