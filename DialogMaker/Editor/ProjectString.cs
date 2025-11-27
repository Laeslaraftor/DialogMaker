using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using DialogMaker.Editor.Menus;

namespace DialogMaker.Editor
{
    public class ProjectString : ObservableObject, IDisposable
    {
        public ProjectString(ProjectController project, DialogProjectString original)
        {
            Project = project;
            Original = original;

            _variantsConverter = new(this);
            _variants = new(original.Variants, new ObservableList<ProjectStringVariant>(), _variantsConverter);
            Variants = new((ObservableList<ProjectStringVariant>)_variants.SecondCollection);
            AddVariantCommand = new RelayCommand(ExecuteAdd, CanAdd);

            Original.PropertyChanged += OnOriginalPropertyChanged;
            Project.PropertyChanged += OnProjectPropertyChanged;
            Variants.CollectionChanged += OnVariantsCollectionChanged;
        }
        ~ProjectString()
        {
            Dispose();
        }

        public ProjectController Project { get; }
        public DialogProjectString Original { get; }
        public ReferenceReadOnlyList<ProjectStringVariant> Variants { get; }
        public ProjectStringVariant? PreviewVariant
        {
            get
            {
                if (Variants.Count == 0)
                {
                    return null;
                }

                var defaultLanguage = Project.DefaultLanguage;

                if (defaultLanguage == null)
                {
                    return Variants[0];
                }

                foreach (var variant in Variants)
                {
                    if (variant.Language != null && 
                        variant.Language.ProjectId == defaultLanguage.ProjectId)
                    {
                        return variant;
                    }
                }

                return Variants.FirstOrDefault(v => !string.IsNullOrEmpty(v.Text));
            }
        }
        public ICommand AddVariantCommand { get; }
        public string Id
        {
            get => Original.Id;
            set => Original.Id = value;
        }
        public bool IsMinimized
        {
            get => _isMinimized;
            set
            {
                if (_isMinimized != value)
                {
                    _isMinimized = value;
                    InvokePropertyChanged(nameof(IsMinimized));
                }
            }
        }
        public ContextMenu ContextMenu
        {
            get
            {
                field ??= new StringContextMenu(this);
                return field;
            }
        }

        private readonly ProjectStringVariantConverter _variantsConverter;
        private readonly CollectionSynchronizer<DialogProjectStringVariant, ProjectStringVariant> _variants;
        private bool _isMinimized = true;

        #region Управление

        public void Dispose()
        {
            Original.PropertyChanged -= OnOriginalPropertyChanged;
            Project.PropertyChanged -= OnProjectPropertyChanged;
            Variants.CollectionChanged -= OnVariantsCollectionChanged;

            _variants.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Команды

        private bool CanAdd(object? parameter)
        {
            return Variants.Count != Project.Languages.Count;
        }
        private void ExecuteAdd(object? parameter)
        {
            try
            {
                Original.CreateVariant();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion

        #region События

        private void OnOriginalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
#nullable disable
            InvokePropertyChanged(e.PropertyName);
#nullable enable
        }
        private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DefaultLanguage")
            {
                InvokePropertyChanged(nameof(PreviewVariant));
            }
        }
        private void OnVariantsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvokePropertyChanged(nameof(PreviewVariant));
        }

        #endregion
    }
}
