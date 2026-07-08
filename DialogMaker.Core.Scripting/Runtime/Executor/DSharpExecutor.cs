namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public class DSharpExecutor(IDSharpAssembly assembly, int stackSize) : Disposable
    {
        public DSharpExecutor(IDSharpAssembly assembly)
            : this(assembly, DSharpThread.DefaultStackSize)
        {
        }

        public IDSharpAssembly Assembly { get; } = assembly;
        public int StackSize { get; } = stackSize;
    }
}
