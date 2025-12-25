using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DialogMaker.Editor
{
    public class ProjectString : ProjectResourceItem<DialogProjectString>
    {
        public ProjectString(ProjectController project, DialogProjectString original)
            : base(project, original)
        {
            _variantsConverter = new(this);
            _variants = new(original.Variants, new ObservableList<ProjectStringVariant>(), _variantsConverter);
            Variants = new((ObservableList<ProjectStringVariant>)_variants.SecondCollection);
            AddVariantCommand = new(ExecuteAdd, CanAdd);

            Project.PropertyChanged += OnProjectPropertyChanged;
            Variants.CollectionChanged += OnVariantsCollectionChanged;
        }

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
        public RelayCommand AddVariantCommand { get; }
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

        private readonly ProjectStringVariantConverter _variantsConverter;
        private readonly CollectionSynchronizer<DialogProjectStringVariant, ProjectStringVariant> _variants;
        private bool _isMinimized = true;

        #region Управление

        public override bool ContainsValue(string value)
        {
            if (base.ContainsValue(value))
            {
                return true;
            }

            foreach (var variant in Variants)
            {
                if (variant.Text.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override ItemContextMenu CreateContextMenu()
        {
            return new StringContextMenu(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Project.PropertyChanged -= OnProjectPropertyChanged;
            Variants.CollectionChanged -= OnVariantsCollectionChanged;

            foreach (var variant in Variants)
            {
                variant.Dispose();
            }

            _variants.Dispose();
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

        private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DefaultLanguage")
            {
                InvokePropertyChanged(nameof(PreviewVariant));
            }
        }
        private void OnVariantsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                AddVariantCommand.InvokeCanExecuteChanged();
            }
            catch (Exception error)
            {
                error.Alert();
            }

            InvokePropertyChanged(nameof(PreviewVariant));
        }

        #endregion
    }
}
