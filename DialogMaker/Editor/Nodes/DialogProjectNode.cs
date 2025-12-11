using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public class DialogProjectNode : ProjectStructureItem
    {
        public DialogProjectNode(ProjectDialog dialog, DialogProjectDialogNode node) 
            : base(dialog.Project, dialog.Original)
        {
            var nodeType = node.GetType();

            Dialog = dialog;
            Original = node;
            Position = new(node.Position.X, node.Position.Y);
            _name = nodeType.GetName();
            Description = nodeType.GetDescription();
            Inputs = new(DialogProjectNodePortProxy.GetInputs(this));
            Outputs = new(DialogProjectNodePortProxy.GetOutputs(this));
            Properties = new(DialogProjectNodeProperty.GetProperties(this));

            node.PropertyChanged += OnNodePropertyChanged;
        }

        public ProjectDialog Dialog { get; }
        public DialogProjectDialogNode Original { get; }
        public override string Icon => Icons.Node;
        public override string Name
        {
            get => _name;
            set { }
        }
        public string Description { get; }
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
                }

                return _view;
            }
        }

        private readonly string _name;
        private DiagramNode? _view;

        #region Управление

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

        #endregion
    }
}
