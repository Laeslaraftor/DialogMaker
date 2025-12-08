using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramNode : UserControl
    {
        public DiagramNode()
        {
            InitializeComponent();
            RenderTransform = _translation;
        }

        public DialogProjectNode? Node
        {
            get => GetValue(NodeProperty) as DialogProjectNode;
            set => SetValue(NodeProperty, value);
        }

        private readonly TranslateTransform _translation = new();
        private readonly ElementsPool<DiagramNodePort> _ports = new();
        private readonly Dictionary<DialogProjectNodePortProxy, DiagramNodePort> _inputPorts = [];
        private readonly Dictionary<DialogProjectNodePortProxy, DiagramNodePort> _outputPorts = [];

        #region Управление

        private async void SetNode(DialogProjectNode? oldValue, DialogProjectNode? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            Clear(_inputPorts.Keys);
            Clear(_outputPorts.Keys);

            _inputs.Children.Clear();
            _outputs.Children.Clear();
            _properties.Children.Clear();
            _inputPorts.Clear();
            _outputPorts.Clear();

            _properties.Width = double.NaN;

            while (_properties.Children.Count > 0)
            {
                await Task.Delay(50);
            }

            ToolTip = newValue?.Description;
            _title.Text = newValue?.Name;
            Point position = (newValue?.Position).GetValueOrDefault();

            _translation.X = position.X;
            _translation.Y = position.Y;

            if (newValue == null)
            {
                return;
            }

            foreach (var port in newValue.Inputs)
            {
                var view = GetInput(port);
                _inputs.Children.Add(view);
            }
            foreach (var port in newValue.Outputs)
            {
                var view = GetOutput(port);
                _outputs.Children.Add(view);
            }
            foreach (var property in newValue.Properties)
            {
                property.View.RemoveFromParent();
                _properties.Children.Add(property.View);
            }

            _properties.Width = newValue.Properties.Count > 0 ? 150 : 0;
        }

        private DiagramNodePort GetOutput(DialogProjectNodePortProxy port)
        {
            return GetNode(_outputPorts, port);
        }
        private DiagramNodePort GetInput(DialogProjectNodePortProxy port)
        {
            return GetNode(_inputPorts, port);
        }
        private bool TryGetPortView(DialogProjectNodePortProxy port, [NotNullWhen(true)] out DiagramNodePort? result)
        {
            if (_inputPorts.TryGetValue(port, out result))
            {
                return true;
            }
            if (_outputPorts.TryGetValue(port, out result))
            {
                return true;
            }

            return false;
        }
        private void Clear(IEnumerable<DialogProjectNodePortProxy> ports)
        {
            foreach (var port in ports)
            {
                port.PropertyChanged -= OnPortPropertyChanged;
            }
        }

        private DiagramNodePort GetNode(Dictionary<DialogProjectNodePortProxy, DiagramNodePort> ports, DialogProjectNodePortProxy port)
        {
            if (!ports.TryGetValue(port, out var result))
            {
                result = _ports.GetElement();
                Setup(port, result);

                port.PropertyChanged += OnPortPropertyChanged;

                ports.Add(port, result);
            }

            return result;
        }
        private void Setup(DialogProjectNodePortProxy port, DiagramNodePort view)
        {
            view.ToolTip = port.Description;
            view.Text = port.Name;
            view.Color = port.Color;
            view.IsActive = port.IsActive;
            view.Invert = port.Original is DialogProjectNodeInput;

            HorizontalAlignment alignment = HorizontalAlignment.Right;

            if (view.Invert)
            {
                alignment = HorizontalAlignment.Left;
            }

            view.HorizontalAlignment = alignment;
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DialogProjectNodePortProxy port &&
                TryGetPortView(port, out var view))
            {
                Setup(port, view);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            Node?.Position = new(_translation.X, _translation.Y);
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
