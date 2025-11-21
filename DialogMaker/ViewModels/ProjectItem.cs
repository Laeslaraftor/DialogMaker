using DialogMaker.Core;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DialogMaker.ViewModels
{
    public class ProjectItem : ObservableObject
    {
        public string Icon
        {
            get => (field) ?? string.Empty;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(Icon));
                }
            }
        }
        public string Name
        {
            get => (field) ?? string.Empty;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }
        public object? Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        public ContextMenu? ContextMenu
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(ContextMenu));
                }
            }
        }
        public ObservableCollection<ProjectItem> Children { get; } = [];

        #region События

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            var instance = Value;

            if (instance == null)
            {
                return;
            }

            var thisProperty = GetType().GetProperty(propertyName);
            var property = instance.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite && thisProperty != null && thisProperty.CanRead)
            {
                property.SetValue(instance, thisProperty.GetValue(this));
            }
        }

        #endregion
    }
}
