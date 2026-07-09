namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# thread
    /// </summary>
    /// <param name="executor">D# virtual machine</param>
    /// <param name="stackCapacity">Stack capacity in items</param>
    public class DSharpThread(DSharpVm executor, int stackCapacity) : Disposable
    {
        /// <summary>
        /// D# virtual machine
        /// </summary>
        public DSharpVm Executor { get; } = executor;
        /// <summary>
        /// Current thread stack
        /// </summary>
        public DSharpStack Stack { get; } = new(stackCapacity);

        #region Controls

        public void Start(IDSharpMethodInfo entry)
        {
        }

        #endregion

        #region Constants

        /// <summary>
        /// Default size of stack for thread in frames.
        /// 1KB per frame
        /// </summary>
        public const int DefaultStackCapacity = 1024;

        #endregion
    }
}
