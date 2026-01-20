using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;
using DialogMaker.Editor;
using DialogMaker.Lib;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using DialogMaker.ViewModels;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Threading.Tasks;
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
            _model.ExportProjectCommand = new RelayCommand(ExecuteExportProject);
            _resourcesDragAndDrop = new(this);

            DataContext = _model;
            Instance = this;
        }

        private readonly MainWindowViewModel _model = new()
        {
            DefaultLanguageVisibility = Visibility.Collapsed
        };
        private readonly ResourcesDragAndDropController _resourcesDragAndDrop;
        private readonly ExportView _exportView = new();

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
            _exportView.ProjectController = controller;
            _model.Project = controller;
            _model.CanCreatePack = controller != null;
            _model.CreatePackCommand = controller?.CreatePackCommand;
            _model.Languages = controller?.Languages;
            _model.GlobalResources = controller?.Resources;

            if (project == null)
            {
                _model.DefaultLanguageVisibility = Visibility.Collapsed;
            }
            if (controller != null)
            {
                _itemTabs.Items.Insert(0, controller);
                _itemTabs.CurrentItem = controller;
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
        private async void ExecuteExportProject(object? parameter)
        {
            if (_exportView.ProjectController == null)
            {
                return;
            }

            Window window = new()
            {
                Title = "Экспорт проекта",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 600,
                Height = 400,
                Content = _exportView
            };

            window.ShowDialog();
            window.Content = null;
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
            if (e.NewValue is IItemTab item)
            {
                _itemTabs.CurrentItem = item;
            }
        }
        private void OnItemTabsCurrentItemChanged(object sender, ValueChangedEventArgs<IItemTab> e)
        {
            if (e.NewValue is IActionsItemTab actionItem)
            {
                _actionButtons.ItemsSource = actionItem.Actions;
                _actionButtonsContainer.Visibility = Visibility.Visible;
                return;
            }

            _actionButtons.ItemsSource = null;
            _actionButtonsContainer.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Статика

#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable enable

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CompileCurrentDialog();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }
        private void CompileCurrentDialog()
        {
            if (_model.Project == null ||
                _itemTabs.CurrentItem is not ProjectDialog dialog)
            {
                return;
            }

            var compiler = DialogCompiler.Create(dialog.Original);
            var result = compiler.Compile();

            DialogCompilerView view = new()
            {
                Builder = compiler.CodeBuilder,
                Code = result,
                ResourcesController = new(new(_model.Project.Project, result.Context.Build()))
            };
            ModalWindow window = new()
            {
                Child = view,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Buttons = ModalWindowButtons.Main,
                MainButtonContent = "Закрыть",
                SizeToContent = SizeToContent.WidthAndHeight
            };

            window.ButtonClick += OnWindowButtonClick;

            void OnWindowButtonClick(object? sender, ClickValueEventArgs<ModalWindowButtons> e)
            {
                window.ButtonClick -= OnWindowButtonClick;
                window.Child = null;
                window.Close();
            }

            window.Show();
        }
    }
}