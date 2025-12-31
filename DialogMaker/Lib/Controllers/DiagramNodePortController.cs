using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public class DiagramNodePortController : Disposable
    {
        public DiagramNodePortController(DialogProjectNodePortProxy port)
        {
            Port = port;

            if (TryCreatePresetValueEditor(port, out var presetValueEditor))
            {
                PresetValueEditor = presetValueEditor;
                View.ExtraControl = presetValueEditor.View;
            }

            port.PropertyChanged += OnPortPropertyChanged;
            port.Original.PropertyChanged += OnPortPropertyChanged;

            Update();
        }

        public DialogProjectNodePortProxy Port { get; }
        public PropertyEditorController? PresetValueEditor { get; }
        public DiagramNodePort View => Port.View;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (PresetValueEditor != null)
            {
                if (View.ExtraControl == PresetValueEditor.View)
                {
                    View.ExtraControl = null;
                }

                PresetValueEditor.Dispose();
            }

            Port.PropertyChanged -= OnPortPropertyChanged;
            Port.Original.PropertyChanged -= OnPortPropertyChanged;
        }

        private void Update()
        {
            bool invert = Port.Original is DialogProjectNodeInput;

            View.DataContext = Port;
            View.Text = Port.Name;
            View.Color = Port.Color;
            View.IsActive = Port.IsActive;
            View.ToolTip = string.IsNullOrEmpty(Port.Description) ? null : Port.Description;
            View.Invert = invert;
            View.HorizontalAlignment = invert ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            View.IsExtraControlVisible = PresetValueEditor != null && !Port.IsActive;
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        #endregion

        #region Статика

        private static bool TryCreatePresetValueEditor(DialogProjectNodePortProxy port, [NotNullWhen(true)] out PropertyEditorController? result)
        {
            result = null;

            if (port.Original.DataType == DialogNodePortType.Object ||
                port.Original.DataType == DialogNodePortType.Action ||
                port.Original is not IValuePort valuePort ||
                !valuePort.CanPresetValue)
            {
                return false;
            }

            var valueProperty = valuePort.GetType().GetProperty("Value", typeof(object));

            if (valueProperty == null)
            {
                return false;
            }

            var propertyType = port.Original.DataType.GetInfo().FirstOrDefault()?.Type;

            if (propertyType == null)
            {
                return false;
            }

            return PropertyEditorController.TryCreate(port.Original, valueProperty, propertyType, out result);
        }

        #endregion
    }
}
