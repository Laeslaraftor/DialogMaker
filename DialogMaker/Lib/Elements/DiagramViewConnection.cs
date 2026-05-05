using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib.Elements
{
    public class DiagramViewConnection : INotifyPropertyChanged, IDisposable
    {
        public DiagramViewConnection(Canvas canvas, UIElement source, UIElement destination)
        {
            Canvas = canvas;
            Source = source;
            Destination = destination;
        }
        ~DiagramViewConnection()
        {
            Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDisposed
        {
            get => _isDisposed;
            private set
            {
                if (_isDisposed != value)
                {
                    _isDisposed = value;
                    OnPropertyChanged(nameof(IsDisposed));
                }
            }
        }
        public Canvas Canvas { get; }
        public UIElement Source { get; }
        public UIElement Destination { get; }
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private bool _isDisposed;
        private Color _color = Colors.Blue;

        #region Управление

        public void Update()
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException("Невозможно обновить удалённую связь");
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
        }

        #endregion

        #region События

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        #endregion
    }
}
