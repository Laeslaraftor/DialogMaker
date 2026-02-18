using DialogMaker.Editor;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramNode : UserControl, ISelectable
    {
        public DiagramNode()
        {
            InitializeComponent();
        }

        public event EventHandler? Redraw;

        public DialogProjectNode? Node
        {
            get => GetValue(NodeProperty) as DialogProjectNode;
            set => SetValue(NodeProperty, value);
        }
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public FrameworkElement? View => this;
        public int GroupCount
        {
            get
            {
                var node = Node;

                if (node != null)
                {
                    return node.GroupCount;
                }

                return 0;
            }
        }

        private bool _lastInvertedValue;

        #region Управление

        public Rect GetViewRect(Visual container)
        {
            Point position = this.GetPosition(container);
            Point scale = this.GetVisualTreeScale();

            return new(position, RenderSize * scale);
        }
        public IEnumerable<ISelectable> GetOtherSelectables()
        {
            var node = Node;

            if (node != null)
            {
                return node.GetOtherSelectables();
            }

            return LibExtensions.EmptyEnumerable<ISelectable>();
        }

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
                oldValue.PropertyChanged -= OnNodePropertyChanged;
                oldValue.Inputs.CollectionChanged -= OnInputsCollectionChanged;
                oldValue.Outputs.CollectionChanged -= OnOutputsCollectionChanged;
            }

            _inputs.Children.Clear();
            _outputs.Children.Clear();
            _properties.Children.Clear();

            _properties.Width = double.NaN;

            while (_properties.Children.Count > 0)
            {
                await Task.Delay(50);
            }

            _title.Text = newValue?.Name;
            Point position = (newValue?.Position).GetValueOrDefault();

            Canvas.SetElementPosition(this, position);

            if (newValue == null)
            {
                return;
            }

            SetInverted(newValue.Inverted);

            PreparePort(_inputs, newValue.Inputs);
            PreparePort(_outputs, newValue.Outputs);

            newValue.Inputs.CollectionChanged += OnInputsCollectionChanged;
            newValue.Outputs.CollectionChanged += OnOutputsCollectionChanged;

            foreach (var property in newValue.Properties)
            {
                property.View.RemoveFromParent();
                _properties.Children.Add(property.View);
            }

            _properties.Width = newValue.Properties.Count > 0 ? double.NaN : 0;
            _properties.MinWidth = newValue.Properties.Count > 0 ? 150 : 0;

            newValue.PropertyChanged += OnNodePropertyChanged;
        }

        private void PreparePort(Panel panel, IEnumerable<DialogProjectNodePortProxy> ports)
        {
            foreach (var port in ports)
            {
                PreparePort(panel, port);
            }
        }
        private void PreparePort(Panel panel, DialogProjectNodePortProxy port)
        {
            var view = port.View;

            view.RemoveFromParent();
            panel.Children.Add(view);

            view.HorizontalAlignment = HorizontalAlignment.Stretch;
        }
        private async void SetInverted(bool value)
        {
            if (_lastInvertedValue == value)
            {
                return;
            }

            _lastInvertedValue = value;
            int inputsColumn = 0;
            Thickness inputsMargin = new(-4, 0, 4, 0);
            int outputsColumn = 2;
            Thickness outputsMargin = new(4, 0, -4, 0);

            if (value)
            {
                (inputsColumn, outputsColumn) = (outputsColumn, inputsColumn);
                (inputsMargin, outputsMargin) = (outputsMargin, inputsMargin);
            }

            _inputs.Margin = inputsMargin;
            _outputs.Margin = outputsMargin;

            Grid.SetColumn(_inputs, inputsColumn);
            Grid.SetColumn(_outputs, outputsColumn);

            await Task.Delay(10);

            Redraw?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region События

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Node?.Position = Canvas.GetElementPosition(this);
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Redraw?.Invoke(this, EventArgs.Empty);
        }

        private void OnOutputsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePorts(this, _outputs, e);
        }
        private void OnInputsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePorts(this, _inputs, e);
        }
        private void OnInvertButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Node?.Inverted ^= true;
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DialogProjectNode node)
            {
                return;
            }
            if (e.PropertyName == "Position")
            {
                Canvas.SetElementPosition(this, node.Position);
            }
            else if (e.PropertyName == nameof(IsSelected) && 
                     IsSelected != node.IsSelected)
            {
                IsSelected = node.IsSelected;
            }
            else if (e.PropertyName == "Inverted")
            {
                SetInverted(node.Inverted);
            }
        }

        private static void OnNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramNode view)
            {
                view.SetNode(e.OldValue as DialogProjectNode, e.NewValue as DialogProjectNode);
            }
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DiagramNode view || e.NewValue is not bool value)
            {
                return;
            }

            Visibility visibility = Visibility.Collapsed;
            var node = view.Node;

            if (value)
            {
                visibility = Visibility.Visible;
            }
            if (node != null && node.IsSelected != value)
            {
                node.IsSelected = value;
            }

            view._selectionOutline.Visibility = visibility;
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register(nameof(Node), typeof(DialogProjectNode),
            typeof(DiagramNode), new(OnNodeChanged));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool),
            typeof(DiagramNode), new(OnIsSelectedChanged));

        #endregion

        #region Статика

        private static void UpdatePorts(DiagramNode view, Panel container, NotifyCollectionChangedEventArgs e)
        {
            void AddPorts(IList? items)
            {
                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (item is DialogProjectNodePortProxy port)
                    {
                        view.PreparePort(container, port);
                    }
                }
            }
            void RemovePorts(IList? items)
            {
                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (item is DialogProjectNodePortProxy port)
                    {
                        if (!port.IsDisposed)
                        {
                            port.View.RemoveFromParent();
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                container.Children.Clear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddPorts(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                RemovePorts(e.OldItems);
                AddPorts(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var currentChild = container.Children[e.OldStartingIndex];
                var movedChild = container.Children[e.NewStartingIndex];

                container.Children.Remove(currentChild);
                container.Children.Remove(movedChild);
                container.Children.Insert(e.NewStartingIndex, currentChild);
                container.Children.Insert(e.OldStartingIndex, movedChild);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemovePorts(e.OldItems);
            }
        }

        #endregion
    }
}
