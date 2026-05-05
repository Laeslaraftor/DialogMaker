using DialogMaker.Lib.Controllers;
using System.ComponentModel;
using System.Windows;

namespace DialogMaker.Editor
{
    public class DialogProjectNodeProperty : Disposable
    {
        protected DialogProjectNodeProperty(DialogProjectNode node, PropertyEditorController controller)
        {
            Node = node;
            _controller = controller;

            controller.PropertyChanged += OnControllerPropertyChanged;
            controller.PropertyChanging += OnControllerPropertyChanging;
        }

        public DialogProjectNode Node { get; }
        public object? Value
        {
            get => _controller.Value;
            set => _controller.Value = value;
        }
        public FrameworkElement View => _controller.View;

        private readonly PropertyEditorController _controller;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _controller.PropertyChanged -= OnControllerPropertyChanged;
            _controller.PropertyChanging -= OnControllerPropertyChanging;

            _controller.Dispose();
        }

        #endregion

        #region События

        private void OnControllerPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(Value))
            {
                OnPropertyChanging(nameof(Value));
            }
        }
        private void OnControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value))
            {
                OnPropertyChanged(nameof(Value));
            }
        }

        #endregion

        #region Статика

        public static List<DialogProjectNodeProperty> GetProperties(DialogProjectNode node)
        {
            var properties = PropertyEditorController.CreateForAllProperties(node.Original);
            List<DialogProjectNodeProperty> result = new(properties.Count);

            foreach (var property in properties)
            {
                result.Add(new(node, property));
            }

            return result;
        }

        #endregion
    }
}
