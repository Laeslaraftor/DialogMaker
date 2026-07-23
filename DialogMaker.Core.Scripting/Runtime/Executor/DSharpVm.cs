using DialogMaker.Core.Scripting.Runtime.Executor.Api;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# virtual machine
    /// </summary>
    public class DSharpVm : Disposable
    {
        /// <param name="runtimeInformationProvider">Runtime information provider</param>
        /// <param name="stackCapacity">Stack capacity in items</param>
        /// <param name="memoryManager">Memory manager</param>
        public DSharpVm(DSharpVmMemoryManager memoryManager, DSharpRuntimeInformationProvider runtimeInformationProvider, int stackCapacity)
        {
            RuntimeTypesProvider = runtimeInformationProvider;
            StackCapacity = stackCapacity;
            _memoryManager = memoryManager;
            _objectContainer = new(runtimeInformationProvider.Assembly, memoryManager, runtimeInformationProvider);
            _multiExternalMethodsProviders = new();
            _standardLibraryExternalMethodsProvider = new(_objectContainer);
            _multiExternalMethodsProviders.Providers.Add(_standardLibraryExternalMethodsProvider);
        }

        /// <summary>
        /// Create new instance of D# virtual machine
        /// </summary>
        /// <param name="assembly">Assembly to executing</param>
        /// <param name="memoryManager">Memory manager</param>
        /// <param name="stackCapacity">Stack capacity in items</param>
        public DSharpVm(IDSharpAssembly assembly, DSharpVmMemoryManager memoryManager, int stackCapacity)
            : this(memoryManager, new DSharpRuntimeInformationProvider(assembly, memoryManager), stackCapacity)
        {
        }
        /// <summary>
        /// Create new instance of D# virtual machine
        /// </summary>
        /// <param name="assembly">Assembly to executing</param>
        public DSharpVm(IDSharpAssembly assembly)
            : this(assembly, new(), DSharpThread.DefaultStackCapacity)
        {
        }

        /// <summary>
        /// Assembly to executing
        /// </summary>
        public IDSharpAssembly Assembly => RuntimeTypesProvider.Assembly;
        /// <summary>
        /// D# runtime types provider
        /// </summary>
        public DSharpRuntimeInformationProvider RuntimeTypesProvider { get; }
        /// <summary>
        /// Stack capacity in items
        /// </summary>
        public int StackCapacity { get; }
        /// <summary>
        /// External methods provider for injecting custom api
        /// </summary>
        public ObservableCollection<IDSharpExternalMethodsProvider> ExternalMethodsProviders
        {
            get
            {
                if (field == null)
                {
                    field = [];
                    field.CollectionChanged += OnExternalMethodsProvidersCollectionChanged;
                }

                return field;
            }
        }

        private readonly DSharpObjectsContainer _objectContainer;
        private readonly DSharpMultiExternalMethodsProviders _multiExternalMethodsProviders;
        private readonly StandardLibraryExternalMethodsProvider _standardLibraryExternalMethodsProvider;
        private readonly DSharpVmMemoryManager _memoryManager = new();

        /// <summary>
        /// Create new D# thread
        /// </summary>
        /// <returns>D# thread</returns>
        public DSharpThread CreateThread() => new(this, _objectContainer, _memoryManager, _multiExternalMethodsProviders, StackCapacity);

        #region Events handlers

        private void OnExternalMethodsProvidersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            void Add(IList? items)
            {
                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (item is IDSharpExternalMethodsProvider provider)
                    {
                        _multiExternalMethodsProviders.Providers.Add(provider);
                    }
                }
            }
            void Remove(IList? items)
            {
                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (item is IDSharpExternalMethodsProvider provider)
                    {
                        _multiExternalMethodsProviders.Providers.Remove(provider);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Add(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Remove(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                Remove(e.OldItems);
                Add(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Remove(e.OldItems);
            }
        }

        #endregion
    }
}
