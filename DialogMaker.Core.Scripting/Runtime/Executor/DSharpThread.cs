namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public class DSharpThread(DSharpExecutor executor, int stackSize) : Disposable
    {
        public DSharpExecutor Executor { get; } = executor;
        public DSharpStack Stack { get; } = new(executor, stackSize);

        /// <summary>
        /// Default size of stack for thread (1 MB)
        /// </summary>
        public const int DefaultStackSize = 1024 * 1024;
    }
}
