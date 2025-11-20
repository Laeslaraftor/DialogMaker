using System.Collections.ObjectModel;

namespace DialogMaker.ViewModels
{
    public class ProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public ObservableCollection<ProjectItem> Children { get; } = [];
    }
}
