using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# virtual machine
    /// </summary>
    /// <param name="assembly">Assembly to executing</param>
    /// <param name="stackCapacity">Stack capacity in items</param>
    public class DSharpVm(DSharpRuntimeTypesProvider runtimeTypesProvider, int stackCapacity) : Disposable
    {
        public DSharpVm(IDSharpAssembly assembly, int stackCapacity)
            : this(new DSharpRuntimeTypesProvider(assembly), stackCapacity)
        {
        }
        public DSharpVm(IDSharpAssembly assembly)
            : this(assembly, DSharpThread.DefaultStackCapacity)
        {
        }

        /// <summary>
        /// Assembly to executing
        /// </summary>
        public IDSharpAssembly Assembly => RuntimeTypesProvider.Assembly;
        /// <summary>
        /// D# runtime types provider
        /// </summary>
        public DSharpRuntimeTypesProvider RuntimeTypesProvider { get; } = runtimeTypesProvider;
        /// <summary>
        /// Stack capacity in items
        /// </summary>
        public int StackCapacity { get; } = stackCapacity;

        private readonly DSharpObjectsContainer _objectContainer = new();
    }
}
