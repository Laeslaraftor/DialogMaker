using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor.Menus;
using DialogMaker.Editor.Nodes;
using DialogMaker.Lib;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace DialogMaker.Editor
{
    public class ProjectDialog : ProjectStructureItem
    {
        public ProjectDialog(ProjectPack pack, DialogProjectDialog dialog) : base(pack.Project, dialog)
        {
            Original = dialog;
            Pack = pack;
            Clipboard = new(this);

            _nodesConverter = new(this);
            _nodesSync = new(dialog.Nodes, Nodes, _nodesConverter);

            dialog.PropertyChanged += OnDialogPropertyChanged;
            SelectedNodes.ItemChanged += OnSelectedNodesItemChanged;
        }

        public DialogProjectDialog Original { get; }
        public ProjectPack Pack { get; }
        public Point LastMouseClickPosition
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(LastMouseClickPosition));
                    field = value;
                    InvokePropertyChanged(nameof(LastMouseClickPosition));
                }
            }
        }
        public override ProjectResources Resources
        {
            get
            {
                _resources ??= new(Project, Original.Resources, Pack.Resources);
                return _resources;
            }
        }
        public override string Icon => Icons.Message;
        public override string Name
        {
            get => Original.Name;
            set => Original.Name = value;
        }
        public override ContextMenu? ContextMenu
        {
            get
            {
                field ??= new DialogContextMenu(Original);
                return field;
            }
        }
        public ContextMenu EditorContextMenu
        {
            get
            {
                field ??= new DialogEditorContextMenu(this);
                return field;
            }
        }
        public override IEnumerable? Children => Nodes;
        public EditableCollection<DialogProjectNode> Nodes { get; } = [];
        public EditableCollection<DialogProjectNode> SelectedNodes { get; } = [];
        public ProjectNodesClipboard Clipboard { get; }
        public DialogProjectNode this[INode originalNode]
        {
            get
            {
                foreach (var node in Nodes)
                {
                    if (node.Original == originalNode)
                    {
                        return node;
                    }
                }

                throw new ArgumentException($"Не удалось найти узел для {originalNode}", nameof(originalNode));
            }
        }

        private readonly ProjectNodeConverter _nodesConverter;
        private readonly CollectionSynchronizer<DialogProjectDialogNode, DialogProjectNode> _nodesSync;
        private ProjectResources? _resources;

        #region Управление

        public bool RemoveSelectedNodes()
        {
            if (SelectedNodes.Count == 0)
            {
                return false;
            }

            List<DialogProjectNode> nodes = [.. SelectedNodes];

            foreach (var node in nodes)
            {
                Original.RemoveNode(node.Original);
            }

            SelectedNodes.Clear();

            return true;
        }

        public IEnumerable<DialogProjectNodePortProxy> GetConnections(DialogProjectNodePortProxy port)
        {
            foreach (var node in Nodes)
            {
                if (node.Original == port.Original.Node)
                {
                    continue;
                }

                foreach (var input in node.Inputs)
                {
                    if (input.Original.IsConnected(port.Original))
                    {
                        yield return input;
                    }
                }
                foreach (var output in node.Outputs)
                {
                    if (output.Original.IsConnected(port.Original))
                    {
                        yield return output;
                    }
                }
            }
        }
        public IEnumerable<KeyValuePair<DialogProjectNodePortProxy, List<DialogProjectNodePortProxy>>> GetPairConnections(DialogProjectNode? connectedNode)
        {
            if (connectedNode == null)
            {
                return GetPairConnections();
            }

            Dictionary<DialogProjectNodePortProxy, List<DialogProjectNodePortProxy>> connections = [];

            void AddOutputs(DialogProjectNode node)
            {
                foreach (var output in node.Outputs)
                {
                    connections.TryAdd(output, []);
                }
            }
            DialogProjectNodePortProxy FindProxy(DialogProjectNodePort port)
            {
                foreach (var node in Nodes)
                {
                    if (node.Original == port.Node)
                    {
                        foreach (var proxy in node.GetPorts())
                        {
                            if (proxy.Original == port)
                            {
                                return proxy;
                            }
                        }
                    }
                }

                throw new ArgumentException($"Не удалось найти порт {port}", nameof(port));
            }

            AddOutputs(connectedNode);

            foreach (var port in connectedNode.Inputs)
            {
                foreach (var output in port.Original)
                {
                    try
                    {
                        connections.Add(FindProxy(output), [port]);
                    }
                    catch (Exception error)
                    {
                        error.Alert();
                    }
                }
            }
            foreach (var port in connectedNode.Outputs)
            {
                var connectionsList = connections[port];

                foreach (var input in port.Original)
                {
                    connectionsList.Add(FindProxy(input));
                }
            }

            return connections;
        }
        public IEnumerable<KeyValuePair<DialogProjectNodePortProxy, List<DialogProjectNodePortProxy>>> GetPairConnections()
        {
            Dictionary<DialogProjectNodePortProxy, List<DialogProjectNodePortProxy>> connections = [];

            void CheckPort(DialogProjectNodePortProxy port)
            {
                foreach (var info in connections)
                {
                    if (info.Key.Original.IsConnected(port.Original))
                    {
                        info.Value.Add(port);
                    }
                }
            }

            foreach (var node in Nodes)
            {
                foreach (var output in node.Outputs)
                {
                    connections.Add(output, []);
                }

            }
            foreach (var node in Nodes)
            {
                foreach (var port in node.Inputs)
                {
                    CheckPort(port);
                }
            }

            foreach (var info in connections)
            {
                if (info.Value.Count > 0)
                {
                    yield return info;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _nodesSync.Dispose();
            _resources?.Dispose();
            _resources = null;

            Original.PropertyChanged -= OnDialogPropertyChanged;
            SelectedNodes.ItemChanged -= OnSelectedNodesItemChanged;
        }

        #endregion

        #region События

        private void OnDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Name))
            {
                InvokePropertyChanged(nameof(Name));
            }
        }
        private void OnSelectedNodesItemChanged(object? sender, CollectionItemEventArgs<DialogProjectNode> e)
        {
            if (e.Action == CollectionItemAction.Add && !Nodes.Contains(e.Item))
            {
                SelectedNodes.Remove(e.Item);
            }
        }

        #endregion
    }
}
