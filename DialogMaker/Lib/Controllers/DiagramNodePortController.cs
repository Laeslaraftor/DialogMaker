using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public class DiagramNodePortController : IDisposable
    {
        public DiagramNodePortController(DialogProjectNodePortProxy port)
        {
            Port = port;
            port.PropertyChanged += OnPortPropertyChanged;
            port.Original.PropertyChanged += OnPortPropertyChanged;

            Update();
        }
        ~DiagramNodePortController()
        {
            Dispose();
        }

        public DialogProjectNodePortProxy Port { get; }
        public DiagramNodePort View => Port.View;

        #region Управление

        public void Dispose()
        {
            Port.PropertyChanged -= OnPortPropertyChanged;
            Port.Original.PropertyChanged -= OnPortPropertyChanged;

            GC.SuppressFinalize(this);
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
        }

        #endregion

        #region События

        private void OnPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        #endregion
    }
}
