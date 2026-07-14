using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# virtual machine
    /// </summary>
    /// <param name="runtimeInformationProvider">Runtime information provider</param>
    /// <param name="stackCapacity">Stack capacity in items</param>
    public class DSharpVm(DSharpRuntimeInformationProvider runtimeInformationProvider, int stackCapacity) : Disposable
    {
        /// <summary>
        /// Create new instance of D# virtual machine
        /// </summary>
        /// <param name="assembly">Assembly to executing</param>
        /// <param name="stackCapacity">Stack capacity in items</param>
        public DSharpVm(IDSharpAssembly assembly, int stackCapacity)
            : this(new DSharpRuntimeInformationProvider(assembly), stackCapacity)
        {
        }
        /// <summary>
        /// Create new instance of D# virtual machine
        /// </summary>
        /// <param name="assembly">Assembly to executing</param>
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
        public DSharpRuntimeInformationProvider RuntimeTypesProvider { get; } = runtimeInformationProvider;
        /// <summary>
        /// Stack capacity in items
        /// </summary>
        public int StackCapacity { get; } = stackCapacity;
        /// <summary>
        /// External methods provider for injecting custom api
        /// </summary>
        public IDSharpExternalMethodsProvider? ExternalMethodsProvider { get; set; }

        private readonly DSharpObjectsContainer _objectContainer = new();
    }
}
