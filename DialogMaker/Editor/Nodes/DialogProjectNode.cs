using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            Inputs = new(DialogProjectNodePortProxy.GetInputs(node));
            Outputs = new(DialogProjectNodePortProxy.GetOutputs(node));
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

        private readonly string _name;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

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
