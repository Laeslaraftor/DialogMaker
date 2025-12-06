using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace DialogMaker.Editor
{
    public class DialogProjectNode : ObservableObject, IDisposable
    {
        public DialogProjectNode(ProjectController project, DialogProjectDialogNode node)
        {
            var nodeType = node.GetType();

            Project = project;
            Original = node;
            Position = new(node.Position.X, node.Position.Y);
            Name = nodeType.GetName();
            Description = nodeType.GetDescription();
            Inputs = new(DialogProjectNodePortProxy.GetInputs(node));
            Outputs = new(DialogProjectNodePortProxy.GetOutputs(node));
            Properties = new(DialogProjectNodeProperty.GetProperties(this));

            node.PropertyChanged += OnNodePropertyChanged;
        }
        ~DialogProjectNode()
        {
            Dispose();
        }

        public ProjectController Project { get; }
        public DialogProjectDialogNode Original { get; }
        public string Name { get; }
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

        #region Управление

        public void Dispose()
        {
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

            GC.SuppressFinalize(this);
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
