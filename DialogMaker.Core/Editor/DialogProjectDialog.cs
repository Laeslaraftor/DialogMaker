using DialogMaker.Core.Editor.Nodes;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialog : Disposable, IProjectResourcesOwner
    {
        public DialogProjectDialog(DialogProjectPack pack, string id)
            : this(pack, id, true)
        {
        }
        public DialogProjectDialog(DialogProjectPack pack, DialogProjectDialogSavedState savedState)
            : this(pack, savedState.Id, false)
        {
            Name = savedState.Name;
            _savedNodes = savedState.Nodes;
        }
        private DialogProjectDialog(DialogProjectPack pack, string id, bool createResources)
        {
            Pack = pack;
            Id = id;
            Folder = Path.Combine(pack.Folder, DialogsFolder);

            if (createResources)
            {
                Resources = new(this, DialogResourcesFlags.Dialog, id);
            }
            else
            {
                Resources = DialogProjectResources.OpenOrCreate(this, DialogResourcesFlags.Dialog, id);
            }
        }

        public DialogProject Project => Pack.Project;
        public DialogProjectPack Pack { get; }
        public string Folder { get; }
        public string Id { get; }
        public string Name
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Name));
                    field = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public EditableCollection<DialogProjectDialogNode> Nodes
        {
            get
            {
                if (field == null)
                {
                    field = [];

                    if (_savedNodes != null)
                    {
                        Dictionary<DialogProjectDialogNode, DialogProjectDialogNodeSavedState> nodeStates = [];

                        foreach (var node in _savedNodes)
                        {
                            try
                            {
                                var restoredNode = DialogProjectDialogNode.Restore(this, node);
                                nodeStates.Add(restoredNode, node);
                                field.Add(restoredNode);
                            }
                            catch (Exception error)
                            {
                                Logger.Log(error);
                            }
                        }

                        RestoreConnections(nodeStates);
                    }

                    field.ItemChanged += OnNodesItemChanged;
                    IsFullLoaded = true;
                }

                return field;
            }
        }
        public bool IsFullLoaded
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(IsFullLoaded));
                    field = value;
                    OnPropertyChanged(nameof(IsFullLoaded));
                }
            }
        }
        public DialogProjectResources Resources { get; }

        IProjectResourcesOwner? IProjectResourcesOwner.Parent => Pack;
        IResourcesOwner IResourcesOwner.Root => Project;
        IResourcesOwner? IResourcesOwner.Parent => Pack;
        IResourcesContainer IResourcesOwner.Resources => Resources;

        private readonly DialogProjectDialogNodeSavedState[]? _savedNodes;

        #region Управление

        public void Save()
        {
            if (!IsFullLoaded)
            {
                return;
            }

            CheckHelper.CheckIsDisposed(this);

            Resources.Save();

            DialogProjectDialogSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Nodes = [.. Nodes.Select(n => n.Save())]
            };

            FileExtensions.CreateDirectory(Folder);
            string filePath = Path.Combine(Folder, $"{Id}.{JsonData.FileExtension}");

            savedState.Save(filePath);
        }

        public bool TryGetNode(Guid id, [NotNullWhen(true)] out DialogProjectDialogNode? result)
        {
            return Nodes.TryGetValue(n => n.Id == id, out result);
        }
        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;
            return false;
        }

        public DialogProjectDialogNode RestoreNode(DialogProjectDialogNodeSavedState savedState)
        {
            return RestoreNode(savedState, true);
        }
        public IEnumerable<DialogProjectDialogNode> RestoreNode(IEnumerable<DialogProjectDialogNodeSavedState> savedStates, Action<Exception>? exceptionsHandler = null)
        {
            Dictionary<DialogProjectDialogNode, DialogProjectDialogNodeSavedState> nodeStates = [];

            foreach (var node in savedStates)
            {
                DialogProjectDialogNode restoredNode;

                try
                {
                    restoredNode = RestoreNode(node, false);
                    nodeStates.Add(restoredNode, node);
                }
                catch (Exception error)
                {
                    exceptionsHandler?.Invoke(error);
                    continue;
                }

                yield return restoredNode;
            }

            RestoreConnections(nodeStates);
        }
        public DialogProjectDialogNode CreateNode(DialogNodeType type)
        {
            var node = DialogProjectDialogNode.Create(this, type);
            Nodes.Add(node);

            return node;
        }
        public T Create<T>()
            where T : DialogProjectDialogNode
        {
            var node = DialogProjectDialogNode.Create<T>(this);
            Nodes.Add(node);

            return node;
        }
        public bool RemoveNode(DialogProjectDialogNode node)
        {
            return Nodes.Remove(node);
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!IsFullLoaded)
            {
                Resources.Dispose();
                return;
            }

            Nodes.ItemChanged -= OnNodesItemChanged;

            foreach (var node in Nodes)
            {
                node.PropertyChanged -= OnNodePropertyChanged;
                node.Dispose();
            }

            Nodes.Clear();
            Resources.Dispose();
        }

        private DialogProjectDialogNode RestoreNode(DialogProjectDialogNodeSavedState savedState, bool restoreConnections)
        {
            var restoredNode = DialogProjectDialogNode.Restore(this, savedState);
            Nodes.Add(restoredNode);

            if (restoreConnections)
            {
                RestoreConnections(restoredNode, savedState);
            }

            return restoredNode;
        }

        private void RestoreConnections(DialogProjectDialogNode node, DialogProjectDialogNodeSavedState savedState)
        {
            void Restore(DialogProjectDialogNode node, Dictionary<int, DialogProjectNodePortSavedState> ports)
            {
                foreach (var portInfo in ports)
                {
                    if (node.TryGetPort(portInfo.Key, out var port))
                    {
                        RestorePort(port, portInfo.Value.Connections);
                    }
                }
            }
            void RestorePort(DialogProjectNodePort port, Dictionary<Guid, List<int>> connections)
            {
                foreach (var connection in connections)
                {
                    if (connection.Value.Count == 0 ||
                        !TryGetNode(connection.Key, out var connectedNode))
                    {
                        continue;
                    }

                    foreach (var connectedPortId in connection.Value)
                    {
                        if (connectedNode.TryGetPort(connectedPortId, out var connectedPort))
                        {
                            try
                            {
                                port.Connect(connectedPort);
                            }
                            catch (Exception error)
                            {
                                Logger.Log(error);
                            }
                        }
                    }
                }
            }

            if (savedState.Inputs.Count != 0)
            {
                Restore(node, savedState.Inputs);
            }
            if (savedState.Outputs.Count != 0)
            {
                Restore(node, savedState.Inputs);
            }
        }
        private void RestoreConnections(Dictionary<DialogProjectDialogNode, DialogProjectDialogNodeSavedState> nodes)
        {
            foreach (var node in nodes)
            {
                RestoreConnections(node.Key, node.Value);
            }
        }

        #endregion

        #region События

        private void OnNodesItemChanged(object sender, CollectionItemEventArgs<DialogProjectDialogNode> e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (e.Action == CollectionItemAction.Add)
            {
                e.Item.PropertyChanged += OnNodePropertyChanged;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.PropertyChanged -= OnNodePropertyChanged;

                if (!e.Item.IsDisposed)
                {
                    e.Item.Dispose();
                }
            }

            OnPropertyChanged(nameof(Nodes));
        }

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is DialogProjectDialogNode node &&
                e.PropertyName == "IsDisposed")
            {
                Nodes.Remove(node);
            }
        }

        #endregion

        #region Статика

        public const string DialogsFolder = "Dialogs";

        public static DialogProjectDialog Open(DialogProjectPack pack, string dialogFilePath)
        {
            var savedState = SavedState.Restore<DialogProjectDialogSavedState>(dialogFilePath);
            return new(pack, savedState);
        }

        #endregion
    }
}
