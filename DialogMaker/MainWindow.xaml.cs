using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using DialogMaker.Lib;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using DialogMaker.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            _resourcesDragAndDrop = new(this);

            DataContext = _model;
            Instance = this;

            ModalWindow modal = new()
            {
                MainButtonContent = "То самое",
                SecondaryButtonContent = "Отмена",
                Buttons = ModalWindowButtons.All
            };
            modal.Show();
        }

        private readonly MainWindowViewModel _model = new()
        {
            DefaultLanguageVisibility = Visibility.Collapsed
        };
        private readonly ResourcesDragAndDropController _resourcesDragAndDrop;

        #region Управление

        public void ClearFocus()
        {
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(this, null);
        }

        private void SetProject(DialogProject? project)
        {
            if (_model.Project?.Project == project)
            {
                return;
            }

            ProjectController? controller = null;

            if (project != null)
            {
                controller = new(project);
            }

            _model.Project?.Dispose();
            _model.Project = controller;
            _model.CanCreatePack = controller != null;
            _model.CreatePackCommand = controller?.CreatePackCommand;
            _model.Languages = controller?.Languages;
            _model.GlobalResources = controller?.Resources;
            _dialogsTabsContainer.Child = controller?.TabsController.TabControl;

            if (project == null)
            {
                _model.DefaultLanguageVisibility = Visibility.Collapsed;
            }
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

        #region События

        protected override async void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            static bool SimpleIsTextInput(object? obj)
            {
                return obj is TextBox ||
                       obj is RichTextBox;
            }
            static bool IsTextInput(object? obj)
            {
                if (SimpleIsTextInput(obj))
                {
                    return true;
                }

                if (obj is FrameworkElement element)
                {
                    FrameworkElement? parent = element;

                    while (parent != null)
                    {
                        if (SimpleIsTextInput(parent))
                        {
                            return true;
                        }

                        parent = parent.Parent as FrameworkElement;
                    }
                }

                return false;
            }

            IInputElement? pressedElement = null;
            int callbackCount = 0;

            await this.Fetch(e, obj =>
            {
                if (IsTextInput(obj))
                {
                    pressedElement = (IInputElement)obj;
                }
            }, callback =>
            {
                if (callbackCount > 0)
                {
                    return true;
                }

                callbackCount++;

                return false;
            });

            if (!IsTextInput(Keyboard.FocusedElement))
            {
                return;
            }

            if (Keyboard.FocusedElement?.Equals(pressedElement) != true)
            {

                if (pressedElement == null ||
                    !pressedElement.Focus())
                {
                    ClearFocus();
                }
            }
        }

        private void OnProjectStructSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ProjectStructureItem item)
            {
                item.Project.TabsController.AddItem(item);
            }
        }

        #endregion

        #region Статика

#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable enable

        #endregion
    }
}