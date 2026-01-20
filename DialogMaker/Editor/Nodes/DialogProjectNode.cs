using Acly;
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
            Inputs = new(DialogProjectNodePortProxy.GetInputs(this));
            Outputs = new(DialogProjectNodePortProxy.GetOutputs(this));
            Properties = new(DialogProjectNodeProperty.GetProperties(this));

            node.PropertyChanged += OnNodePropertyChanged;
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
                    InvokePropertyChanging(nameof(Position));
                    field = value;
                    Original.Position = new((float)value.X, (float)value.Y);
                    InvokePropertyChanged(nameof(Position));
                }
            }
        }
        public ReadOnlyCollection<DialogProjectNodePortProxy> Inputs { get; }
        public ReadOnlyCollection<DialogProjectNodePortProxy> Outputs { get; }
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
                    InvokePropertyChanging(nameof(IsSelected));
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

                    InvokePropertyChanged(nameof(IsSelected));
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
            bool Search(ReadOnlyCollection<DialogProjectNodePortProxy> ports, [NotNullWhen(true)] out DiagramNodePort? view)
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

            Original.PropertyChanged -= OnNodePropertyChanged;
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

        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Position))
            {
                Position = new(Original.Position.X, Original.Position.Y);
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
    }
}
