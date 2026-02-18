using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using DialogMaker.Lib.InputFields;
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
            View.DataContext = Port;
            View.Text = Port.Name;
            View.Color = Port.Color;
            View.IsActive = Port.IsActive;
            View.ToolTip = string.IsNullOrEmpty(Port.Description) ? null : Port.Description;
            View.Invert = Port.Inverted;
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

            if (port.Original is not IValuePort valuePort ||
                !valuePort.CanPresetValue)
            {
                return false;
            }

            var valueProperty = valuePort.GetType().GetProperty("Value", typeof(object));

            if (valueProperty == null)
            {
                return false;
            }

            Type? propertyType = valuePort.ReflectionValueType;

            if (PropertyEditorController.TryCreate(port.Original, valueProperty, propertyType, out result))
            {
                var newPlaceholder = valuePort.ResourceType?.GetEnumAttribute<NameAttribute>()?.Name;

                if (result.InputField is ObjectInputField objectField)
                {
                    objectField.AllowedValues = valuePort.AllowedValues;
                    objectField.ResourceType ??= valuePort.ResourceType;
                }
                if (result.InputField is ReferenceInputField referenceField)
                {
                    referenceField.ResourceType = valuePort.ResourceType;
                }
                if (newPlaceholder != null)
                {
                    result.InputField.Placeholder = newPlaceholder;
                }

                result.ExtraConverter = PortFieldValueConverter;
            }

            return result != null;
        }

        private static object? PortFieldValueConverter(object? value)
        {
            if (value is ProjectResourceItem resource)
            {
                return resource.Model;
            }

            return value;
        }

        #endregion
    }
}
