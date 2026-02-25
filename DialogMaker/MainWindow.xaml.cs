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
using System.Windows.Media.Imaging;

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
            _backgroundRendererController.RendererChanged += OnBackgroundRendererControllerRendererChanged;

            DataContext = _model;
            Instance = this;
        }

        private readonly MainWindowViewModel _model = new()
        {
            DefaultLanguageVisibility = Visibility.Collapsed
        };
        private readonly ResourcesDragAndDropController _resourcesDragAndDrop;
        private readonly ExportView _exportView = new();
        private readonly ViewRenderController _backgroundRendererController = new()
        {
            PixelFormat = PixelFormats.Pbgra32
        };

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
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _blurredBackgroundMainClipRect.Rect = new(sizeInfo.NewSize);
        }

        private void OnBackgroundRendererControllerRendererChanged(object? sender, ValueChangedEventArgs<RenderTargetBitmap?> e)
        {
            _blurredBackgroundBrush.ImageSource = e.NewValue;
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
                oldDialogView.DiagramViewRedraw -= OnDialogViewDiagramViewRedraw;
            }
            if (e.NewValue?.TabContent is DialogAndResourcesView newDialogView)
            {
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

        private bool _isRendering;

        private async void OnDialogViewDiagramViewRedraw(object? sender, ItemEventArgs<DiagramView> e)
        {
            if (_isRendering || sender is not DialogAndResourcesView dialogView)
            {
                return;
            }

            Point position;

            try
            {
                position = e.Item.TransformToVisual(this).Transform(new(0, 0));
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
                return;
            }

            _isRendering = true;
            //await Task.Delay(100);

            position *= -1;
            PresentationSource source = PresentationSource.FromVisual(this);
            Size dpi = new(96, 96);

            if (source != null)
            {
                dpi.Width *= source.CompositionTarget.TransformToDevice.M11;
                dpi.Height *= source.CompositionTarget.TransformToDevice.M22;
            }

            int sizePart = 2;
            dpi /= sizePart;
            var size = RenderSize / sizePart;

            _backgroundRendererController.Size = size;
            _backgroundRendererController.Dpi = dpi;
            _backgroundRendererController.Render(e.Item.Canvas);

            _blurredBackgroundBrush.Viewbox = new(position, _blurredBackground.RenderSize);
            _blurredBackgroundDiagramClipRect.Rect = dialogView.GetDiagramRect();

            _isRendering = false;
        }

        #endregion

        #region Статика

#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable enable

        #endregion
    }
}