using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public interface IItemTab
    {
        public event EventHandler? CloseRequested;

        public string Name { get; set; }
        public bool CanRename { get; }
        public bool CanClose { get; }
        public UIElement? TabContent { get; }

        public void OnClosed(object? sender, EventArgs e);
        public void OnHided(object? sender, EventArgs e);
        public void OnShowed(object? sender, EventArgs e);
    }
}
