using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using DialogMaker.Lib;
using DialogMaker.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace DialogMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _model.CreateProjectCommand = new RelayCommand(ExecuteCreateProject);
            _model.OpenProjectCommand = new RelayCommand(ExecuteOpenProject);
            _model.CloseProjectCommand = new RelayCommand(ExecuteCloseProject);

            DataContext = _model;
        }

        public ProjectController? CurrentProject { get; private set; }

        private readonly MainWindowViewModel _model = new();

        #region Управление

        private void SetProject(DialogProject? project)
        {
            ProjectController? controller = null;

            if (project != null)
            {
                controller = new(project);
            }

            _model.Project = controller;
            _model.CanCreatePack = controller != null;
            _model.CreatePackCommand = controller?.CreatePackCommand;
            _model.DialogPacks = controller?.Structure;

            if (project != null)
            {
                controller?.Languages.Add(new(project)
                {
                    Id = "ru",
                    Name = "Русский"
                });
            }

            _model.Languages = controller?.Languages;
        }

        #endregion

        #region Команды

        private void ExecuteCreateProject(object? parameter)
        {
            var project = ProjectController.Create();

            if (project != null)
            {
                SetProject(project);
            }
        }
        private void ExecuteOpenProject(object? parameter)
        {
            var project = ProjectController.Open();

            if (project != null)
            {
                SetProject(project);
            }
        }
        private void ExecuteCloseProject(object? parameter)
        {
            SetProject(null);
        }

        #endregion
    }
}