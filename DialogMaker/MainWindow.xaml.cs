using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using DialogMaker.Lib;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using DialogMaker.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            _nodeSelectorController = new(this, _nodeSelector);
            _hotkeysController = new(this);

            DataContext = _model;
            Instance = this;
        }
        ~MainWindow()
        {
            _resourcesDragAndDrop.Dispose();
            _nodeSelectorController.Dispose();
            _hotkeysController.Dispose();
        }

        private readonly MainWindowViewModel _model = new()
        {
            DefaultLanguageVisibility = Visibility.Collapsed
        };
        private readonly ResourcesDragAndDropController _resourcesDragAndDrop;
        private readonly ExportView _exportView = new();
        private readonly NodeSelectorController _nodeSelectorController;
        private readonly HotkeysController _hotkeysController;
        private bool _topMenuIsShowed;

        #region Управление

        public void ClearFocus()
        {
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(this, null);

            FocusManager.SetFocusedElement(this, this);
            Keyboard.Focus(this);
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

            try
            {
                _model.Project?.Dispose();
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }

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

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (_topMenu.IsOpen())
            {
                return;
            }

            var position = e.GetPosition(this);
            UIElement elementToShow = _logoContainer;
            UIElement elementToHide = _menuContainer;

            if (_menuContainer.ActualHeight * 2 > position.Y)
            {
                if (_topMenuIsShowed)
                {
                    return;
                }

                _topMenuIsShowed = true;
                (elementToShow, elementToHide) = (elementToHide, elementToShow);
            }
            else if (!_topMenuIsShowed)
            {
                return;
            }
            else
            {
                _topMenuIsShowed = false;
            }

            AnimationsHelper.FadeIn(elementToShow);
            AnimationsHelper.FadeOut(elementToHide);
        }
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

            DependencyObject? pressedElement = VisualTreeHelper.HitTest(this, e.GetPosition(this)).VisualHit;

            if (!IsTextInput(Keyboard.FocusedElement) || 
                FocusHelper.GetVisualTreeIgnoreFocusSwitch(Keyboard.FocusedElement as DependencyObject))
            {
                return;
            }

            if (Keyboard.FocusedElement?.Equals(pressedElement) != true)
            {
                if (pressedElement != null && !FocusHelper.GetVisualTreeIgnoreFocusHit(pressedElement))
                {
                    if (pressedElement is IInputElement input && input.Focus())
                    {
                        return;
                    }

                    ClearFocus();
                }
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _blurredBackgroundMainClipRect.Rect = new(sizeInfo.NewSize);
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Maximized)
            {
                _mainGrid.Margin = new(8);
                return;
            }

            _mainGrid.Margin = new(0);
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
            if (e.OldValue?.TabContent is DialogAndResourcesView oldDialogView)
            {
                _blurredBackgroundBrush.Visual = null;
                oldDialogView.DiagramViewRedraw -= OnDialogViewDiagramViewRedraw;
            }
            if (e.NewValue?.TabContent is DialogAndResourcesView newDialogView)
            {
                _blurredBackgroundBrush.Visual = newDialogView._diagram.Canvas;
                newDialogView.DiagramViewRedraw += OnDialogViewDiagramViewRedraw;
            }
            if (e.NewValue is IActionsItemTab actionItem && actionItem.Actions != null)
            {
                _actionButtons.ItemsSource = actionItem.Actions;
                _actionButtonsContainer.Visibility = Visibility.Visible;
                return;
            }

            _actionButtons.ItemsSource = null;
            _actionButtonsContainer.Visibility = Visibility.Collapsed;
        }

        private async void OnDialogViewDiagramViewRedraw(object? sender, ItemEventArgs<DiagramView> e)
        {
            if (sender is not DialogAndResourcesView dialogView)
            {
                return;
            }

            var clipRect = dialogView.GetDiagramRect();
            Point offset = new(_mainGrid.Margin.Left, _mainGrid.Margin.Top);
            clipRect.Location = (Point)(clipRect.Location - offset);

            _blurredBackgroundBrush.Viewbox = new(offset, dialogView._diagramSizeReference.RenderSize);
            _blurredBackgroundDiagramClipRect.Rect = clipRect;
        }

        #endregion

        #region Статика

#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable enable

        #endregion
    }
}