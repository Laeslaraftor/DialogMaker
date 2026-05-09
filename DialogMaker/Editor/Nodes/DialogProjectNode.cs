using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Editor
{
    public class DialogProjectNode : ProjectStructureItem, ISelectable
    {
        public DialogProjectNode(ProjectDialog dialog, DialogProjectDialogNode node)
            : base(dialog.Project, dialog.Original)
        {
            var nodeType = node.GetType();

            Dialog = dialog;
            Original = node;
            Position = new(node.Position.X, node.Position.Y);

            _inputs = DialogProjectNodePortProxy.GetInputs(this);
            _outputs = DialogProjectNodePortProxy.GetOutputs(this);

            Inputs = new(_inputs);
            Outputs = new(_outputs);
            Properties = new(DialogProjectNodeProperty.GetProperties(this));

            node.PropertyChanged += OnNodePropertyChanged;
            node.PropertyChanging += OnNodePropertyChanging;
            node.InputsUpdated += OnNodeInputsUpdated;
            node.OutputsUpdated += OnNodeOutputsUpdated;
            dialog.SelectedNodes.ItemChanged += OnSelectedNodesItemChanged;
        }

        public ProjectDialog Dialog { get; }
        public DialogProjectDialogNode Original { get; }
        public override ProjectResources Resources => Dialog.Resources;
        public override string Icon => Icons.Node;
        public override string Name
        {
            get => Original.Name;
            set { }
        }
        public string Description => Original.Description;
        public Point Position
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Position));
                    field = value;
                    Original.Position = new((float)value.X, (float)value.Y);
                    OnPropertyChanged(nameof(Position));
                }
            }
        }
        public bool Inverted
        {
            get => Original.Inverted;
            set => Original.Inverted = value;
        }
        public ReferenceReadOnlyList<DialogProjectNodePortProxy> Inputs { get; }
        public ReferenceReadOnlyList<DialogProjectNodePortProxy> Outputs { get; }
        public ReadOnlyCollection<DialogProjectNodeProperty> Properties { get; }
        public override ContextMenu? ContextMenu => null;
        public override IEnumerable? Children => null;
        public bool IsSelected
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(IsSelected));
                    field = value;
                    bool contains = Dialog.SelectedNodes.Contains(this);

                    if (value && !contains)
                    {
                        Dialog.SelectedNodes.Add(this);
                    }
                    else if (!value && contains)
                    {
                        Dialog.SelectedNodes.Remove(this);
                    }

                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public DiagramNode View
        {
            get
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Элемент очищен, взаимодействие невозможно");
                }
                if (_view == null)
                {
                    _view = Project.NodesViewFabric.GetNode();
                    _view.Node = this;
                    _view.ToolTip = string.IsNullOrEmpty(Description) ? null : Description;
                }

                return _view;
            }
        }
        public int GroupCount => Dialog.SelectedNodes.Count;
        public override UIElement? TabContent => null;
        FrameworkElement? ISelectable.View => View;

        private readonly EditableCollection<DialogProjectNodePortProxy> _inputs = [];
        private readonly EditableCollection<DialogProjectNodePortProxy> _outputs = [];
        private DiagramNode? _view;

        #region Управление

        public Rect GetViewRect(Visual container)
        {
            return View.GetViewRect(container);
        }
        public IEnumerable<ISelectable> GetOtherSelectables()
        {
            foreach (var selectedNode in Dialog.SelectedNodes)
            {
                if (selectedNode != this)
                {
                    yield return selectedNode;
                }
            }
        }
        public IEnumerable<DialogProjectNodePortProxy> GetPorts()
        {
            foreach (var port in Inputs)
            {
                yield return port;
            }
            foreach (var port in Outputs)
            {
                yield return port;
            }
        }

        public bool TryGetPortView(DialogProjectNodePortProxy port, [NotNullWhen(true)] out DiagramNodePort? result)
        {
            bool Search(IEnumerable<DialogProjectNodePortProxy> ports, [NotNullWhen(true)] out DiagramNodePort? view)
            {
                view = null;

                foreach (var p in ports)
                {
                    if (p == port)
                    {
                        view = p.View;
                        return true;
                    }
                }

                return false;
            }

            if (Search(Inputs, out result))
            {
                return true;
            }
            if (Search(Outputs, out result))
            {
                return true;
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (_view != null)
            {
                Project.NodesViewFabric.Free(_view);
                _view = null;
            }

            Original.InputsUpdated -= OnNodeInputsUpdated;
            Original.OutputsUpdated -= OnNodeOutputsUpdated;
            Original.PropertyChanged -= OnNodePropertyChanged;
            Original.PropertyChanging -= OnNodePropertyChanging;
            Dialog.SelectedNodes.ItemChanged -= OnSelectedNodesItemChanged;

            foreach (var input in Inputs)
            {
                input.Dispose();
            }
            foreach (var output in Outputs)
            {
                output.Dispose();
            }
            foreach (var property in Properties)
            {
                property.Dispose();
            }
        }

        #endregion

        #region События

        private void OnNodeOutputsUpdated(object? sender, EventArgs e)
        {
            UpdatePort(this, Original.GetOutputs(), _outputs);
        }
        private void OnNodeInputsUpdated(object? sender, EventArgs e)
        {
            UpdatePort(this, Original.GetInputs(), _inputs);
        }

        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Position))
            {
                Position = new(Original.Position.X, Original.Position.Y);
            }
            else if (e.PropertyName == nameof(Inverted) ||
                     e.PropertyName == nameof(Name))
            {
                OnPropertyChanged(e);
            }
        }
        private void OnNodePropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(Inverted))
            {
                OnPropertyChanging(e);
            }
        }

        private void OnSelectedNodesItemChanged(object? sender, CollectionItemEventArgs<DialogProjectNode> e)
        {
            if (e.Item != this || e.Action == CollectionItemAction.Move)
            {
                return;
            }

            IsSelected = e.Action == CollectionItemAction.Add;
        }

        #endregion

        #region Статика

        private static void UpdatePort<T>(DialogProjectNode node, ReferenceReadOnlyDictionary<T, DialogProjectNodeMetadata> ports, EditableCollection<DialogProjectNodePortProxy> proxyPorts)
            where T : DialogProjectNodePort
        {
            List<DialogProjectNodePortProxy> portsToRemove = [];
            Dictionary<DialogProjectNodePort, DialogProjectNodeMetadata> portsToAdd = [];

            bool Contains(DialogProjectNodePort checkPort)
            {
                foreach (var port in proxyPorts)
                {
                    if (port.Original.Equals(checkPort))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var port in proxyPorts)
            {
                if (!ports.ContainsKey((T)port.Original))
                {
                    portsToRemove.Add(port);
                }
            }
            foreach (var port in ports)
            {
                if (!Contains(port.Key))
                {
                    portsToAdd.Add(port.Key, port.Value);
                }
            }
            foreach (var port in portsToRemove)
            {
                port.Dispose();
                proxyPorts.Remove(port);
            }
            foreach (var port in portsToAdd)
            {
                proxyPorts.Add(new(node, port.Key, port.Value));
            }
        }

        #endregion
    }
}
