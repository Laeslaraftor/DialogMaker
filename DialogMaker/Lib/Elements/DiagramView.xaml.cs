using Acly;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using DialogMaker.Lib.Controllers;
using DragEventArgs = DialogMaker.Lib.Controllers.DragEventArgs;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public partial class DiagramView : UserControl
    {
        public DiagramView()
        {
            InitializeComponent();

            _connections = [];
            _dragAndDrop = new(this);
            Connections = new(_connections);

            _dragAndDrop.DragUpdated += OnDragAndDropDragUpdated;
        }

        public UIElementCollection Children => _canvas.Children;
        public ReferenceReadOnlyList<DiagramViewConnection> Connections { get; }

        private readonly DragAndDropController _dragAndDrop;
        private readonly ObservableList<DiagramViewConnection> _connections;

        #region Управление

        public bool AlreadyConnected(UIElement element1, UIElement element2)
        {
            foreach (var connection in _connections)
            {
                if ((connection.Source == element1 || connection.Source == element2) &&
                    (connection.Destination == element1 || connection.Destination == element2))
                {
                    return true;
                }
            }

            return false;
        }
        public DiagramViewConnection AddConnection(UIElement source, UIElement destination)
        {
            if (AlreadyConnected(source, destination))
            {
                throw new ArgumentException("Связь с этих элементов уже установлена");
            }

            DiagramViewConnection connection = new(_canvas, source, destination);
            connection.PropertyChanged += OnConnectionPropertyChanged; 

            return connection;
        }
        public bool RemoveConnection(DiagramViewConnection connection)
        {
            if (!_connections.Remove(connection))
            {
                return false;
            }

            connection.PropertyChanged -= OnConnectionPropertyChanged;

            if (!connection.IsDisposed)
            {
                connection.Dispose();
            }

            return true;
        }

        private void UpdateCanvasSize()
        {
            double width = RenderSize.Width;
            double height = RenderSize.Height;

            foreach (UIElement child in _canvas.Children)
            {
                if (child.RenderTransform is not TranslateTransform translation)
                {
                    continue;
                }

                width = Math.Max(width, translation.X + child.RenderSize.Width);
                height = Math.Max(height, translation.Y + child.RenderSize.Height);
            }

            _canvas.Width = width;
            _canvas.Height = height;
        }

        #endregion

        #region События

        private void OnConnectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DiagramViewConnection connection && e.PropertyName == "IsDisposed")
            {
                RemoveConnection(connection);
            }
        }

        private void OnDragAndDropDragUpdated(object? sender, DragEventArgs e)
        {
            UpdateCanvasSize();
        }

        #endregion
    }
}
