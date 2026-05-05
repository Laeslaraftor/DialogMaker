using DialogMaker.Editor;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DialogMaker.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public IEnumerable? Languages
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Languages));
                }
            }
        }
        public ICommand? CreateProjectCommand
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(CreateProjectCommand));
                }
            }
        }
        public ICommand? OpenProjectCommand
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(OpenProjectCommand));
                }
            }
        }
        public ICommand? CloseProjectCommand
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(CloseProjectCommand));
                }
            }
        }
        public ICommand? ExportProjectCommand
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ExportProjectCommand));
                }
            }
        }
        public bool CanCreatePack
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(CanCreatePack));
                }
            }
        }
        public ICommand? CreatePackCommand
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(CreatePackCommand));
                }
            }
        }
        public ProjectController? Project
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;

                    if (field != null)
                    {
                        field.PropertyChanged -= OnProjectPropertyChanged;
                    }
                    if (value != null)
                    {
                        value.PropertyChanged += OnProjectPropertyChanged;
                        UpdateProject(value);
                    }

                    OnPropertyChanged(nameof(Project));
                }
            }
        }
        public Visibility DefaultLanguageVisibility
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(DefaultLanguageVisibility));
                }
            }
        }
        public ProjectResources? GlobalResources
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(GlobalResources));
                }
            }
        }

        #region Управление

        private void UpdateProject(ProjectController controller)
        {
            Visibility visibility = Visibility.Collapsed;

            if (controller.IsDefaultLanguageSetted)
            {
                visibility = Visibility.Visible;
            }

            DefaultLanguageVisibility = visibility;
        }

        #endregion

        #region События

        private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ProjectController controller || e.PropertyName != "IsDefaultLanguageSetted")
            {
                return;
            }

            UpdateProject(controller);
        }

        #endregion
    }
}
