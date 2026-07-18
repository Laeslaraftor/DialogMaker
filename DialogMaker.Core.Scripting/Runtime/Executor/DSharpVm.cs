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
        public DSharpVm(DSharpRuntimeInformationProvider runtimeInformationProvider, int stackCapacity)
        {
            RuntimeTypesProvider = runtimeInformationProvider;
            StackCapacity = stackCapacity;
            _objectContainer = new(runtimeInformationProvider.Assembly, runtimeInformationProvider);
            _multiExternalMethodsProviders = new();
            _multiExternalMethodsProviders.Providers.Add(StandardLibraryExternalMethodsProvider.Instance);
        }

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

        /// <summary>
        /// Create new D# thread
        /// </summary>
        /// <returns>D# thread</returns>
        public DSharpThread CreateThread() => new(this, _objectContainer, _multiExternalMethodsProviders, StackCapacity);

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
