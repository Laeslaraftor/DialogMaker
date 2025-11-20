using Acly;
using DialogMaker.Core;
using DialogMaker.Editor;
using System.Windows.Input;

namespace DialogMaker.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ReferenceReadOnlyList<ProjectItem>? DialogPacks
        {
            get => _dialogPacks;
            set
            {
                if (_dialogPacks != value)
                {
                    _dialogPacks = value;
                    InvokePropertyChanged(nameof(DialogPacks));
                }
            }
        }
        public ICommand? CreateProjectCommand
        {
            get => _createProjectCommand;
            set
            {
                if (_createProjectCommand != value)
                {
                    _createProjectCommand = value;
                    InvokePropertyChanged(nameof(CreateProjectCommand));
                }
            }
        }
        public ICommand? OpenProjectCommand
        {
            get => _openProjectCommand;
            set
            {
                if (_openProjectCommand != value)
                {
                    _openProjectCommand = value;
                    InvokePropertyChanged(nameof(OpenProjectCommand));
                }
            }
        }
        public ICommand? CloseProjectCommand
        {
            get => _closeProjectCommand;
            set
            {
                if (_closeProjectCommand != value)
                {
                    _closeProjectCommand = value;
                    InvokePropertyChanged(nameof(CloseProjectCommand));
                }
            }
        }
        public bool CanCreatePack
        {
            get => _canCreatePack;
            set
            {
                if (_canCreatePack != value)
                {
                    _canCreatePack = value;
                    InvokePropertyChanged(nameof(CanCreatePack));
                }
            }
        }
        public ICommand? CreatePackCommand
        {
            get => _createPackCommand;
            set
            {
                if (_createPackCommand != value)
                {
                    _createPackCommand = value;
                    InvokePropertyChanged(nameof(CreatePackCommand));
                }
            }
        }
        public ProjectController? Project
        {
            get => _project;
            set
            {
                if (_project != value)
                {
                    _project = value;
                    InvokePropertyChanged(nameof(Project));
                }
            }
        }

        private ReferenceReadOnlyList<ProjectItem>? _dialogPacks;
        private ICommand? _createProjectCommand;
        private ICommand? _openProjectCommand;
        private ICommand? _closeProjectCommand;
        private bool _canCreatePack;
        private ICommand? _createPackCommand;
        private ProjectController? _project;
    }
}
