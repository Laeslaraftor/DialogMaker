using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using System.Windows.Input;

namespace DialogMaker.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ObservableList<DialogProjectLanguage>? Languages
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
                    InvokePropertyChanged(nameof(Project));
                }
            }
        }
    }
}
