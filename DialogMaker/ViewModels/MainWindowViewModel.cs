using Acly;
using DialogMaker.Core;
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
                    InvokePropertyChanged(nameof(Languages));
                }
            }
        }
        public ReferenceReadOnlyList<ProjectItem>? DialogPacks
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    InvokePropertyChanged(nameof(DialogPacks));
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
                    InvokePropertyChanged(nameof(CreateProjectCommand));
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
                    InvokePropertyChanged(nameof(OpenProjectCommand));
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
                    InvokePropertyChanged(nameof(CloseProjectCommand));
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
                    InvokePropertyChanged(nameof(CanCreatePack));
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
                    InvokePropertyChanged(nameof(CreatePackCommand));
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

                    InvokePropertyChanged(nameof(Project));
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
                    InvokePropertyChanged(nameof(DefaultLanguageVisibility));
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
                    InvokePropertyChanged(nameof(GlobalResources));
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
