using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Controllers;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramNode : UserControl
    {
        public DiagramNode()
        {
            InitializeComponent();
        }

        public event EventHandler<ItemMouseEventArgs<DialogProjectNodePortProxy>>? PortPressed;

        public DialogProjectNode? Node
        {
            get => GetValue(NodeProperty) as DialogProjectNode;
            set => SetValue(NodeProperty, value);
        }

        #region Управление

        public Point GetPortPosition(DialogProjectNodePortProxy port, Visual relativeTo)
        {
            if (TryGetPortView(port, out var view))
            {
                return view.GetConnectorPosition(relativeTo);
            }

            return new();
        }
        public bool TryGetPortView(DialogProjectNodePortProxy port, [NotNullWhen(true)] out DiagramNodePort? result)
        {
            var node = Node;
            result = null;

            if (node != null && node.TryGetPortView(port, out result))
            {
                return true;
            }

            return false;
        }

        private async void SetNode(DialogProjectNode? oldValue, DialogProjectNode? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            if (oldValue != null)
            {
                Clear(oldValue.Inputs);
                Clear(oldValue.Outputs);

                oldValue.PropertyChanged -= OnNodePropertyChanged;
            }

            _inputs.Children.Clear();
            _outputs.Children.Clear();
            _properties.Children.Clear();

            _properties.Width = double.NaN;

            while (_properties.Children.Count > 0)
            {
                await Task.Delay(50);
            }

            ToolTip = newValue?.Description;
            _title.Text = newValue?.Name;
            Point position = (newValue?.Position).GetValueOrDefault();

            Canvas.SetElementPosition(this, position);

            if (newValue == null)
            {
                return;
            }

            foreach (var port in newValue.Inputs)
            {
                _inputs.Children.Add(GetPortView(port));
            }
            foreach (var port in newValue.Outputs)
            {
                _outputs.Children.Add(GetPortView(port));
            }
            foreach (var property in newValue.Properties)
            {
                property.View.RemoveFromParent();
                _properties.Children.Add(property.View);
            }

            _properties.Width = newValue.Properties.Count > 0 ? double.NaN : 0;
            _properties.MinWidth = newValue.Properties.Count > 0 ? 150 : 0;

            newValue.PropertyChanged += OnNodePropertyChanged;
        }
        private bool TryGetPort(DiagramNodePort view, [NotNullWhen(true)] out DialogProjectNodePortProxy? result)
        {
            result = null;

            if (view.DataContext is DialogProjectNodePortProxy model)
            {
                result = model;
                return true;
            }

            return false;
        }
        private void Clear(IEnumerable<DialogProjectNodePortProxy> ports)
        {
            foreach (var port in ports)
            {
                port.View.PreviewMouseDown -= OnPortPreviewMouseDown;
            }
        }
        private DiagramNodePort GetPortView(DialogProjectNodePortProxy port)
        {
            var view = port.View;
            view.RemoveFromParent();

            view.PreviewMouseDown -= OnPortPreviewMouseDown;
            view.PreviewMouseDown += OnPortPreviewMouseDown;

            return view;
        }

        #endregion

        #region События

        private void OnPortPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DiagramNodePort view && TryGetPort(view, out var port))
            {
                PortPressed?.Invoke(this, new(port, e));
            }
        }
        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DialogProjectNode node &&
                e.PropertyName == "Position")
            {
                Canvas.SetElementPosition(this, node.Position);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Node?.Position = Canvas.GetElementPosition(this);
            }
        }

        private static void OnNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNode view)
            {
                view.SetNode(e.OldValue as DialogProjectNode, e.NewValue as DialogProjectNode);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(DialogProjectNode),
            typeof(DiagramNode), new(OnNodeChanged));

        #endregion
    }
}
