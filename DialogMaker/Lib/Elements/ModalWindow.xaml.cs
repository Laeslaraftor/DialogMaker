using DialogMaker.Lib.Controllers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Elements
{
    public partial class ModalWindow : Window
    {
        public ModalWindow()
        {
            InitializeComponent();
        }

        public event EventHandler<ClickValueEventArgs<ModalWindowButtons>>? ButtonClick;

        public object? Child
        {
            get => GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }
        public ModalWindowButtons Buttons
        {
            get => (ModalWindowButtons)GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }
        public object? MainButtonContent
        {
            get => GetValue(MainButtonContentProperty);
            set => SetValue(MainButtonContentProperty, value);
        }
        public object? SecondaryButtonContent
        {
            get => GetValue(SecondaryButtonContentProperty);
            set => SetValue(SecondaryButtonContentProperty, value);
        }
        public bool CanMove
        {
            get => (bool)GetValue(CanMoveProperty);
            set => SetValue(CanMoveProperty, value);
        }

        private Point _startDragWindowPosition;
        private Point _startDragMousePosition;
        private bool _ignoreDrag;

        #region Управление

        private void SetGridValues(UIElement element, int column, int span)
        {
            Grid.SetColumn(element, column);
            Grid.SetColumnSpan(element, span);

            element.Visibility = Visibility.Visible;
        }

        #endregion

        #region События

        protected virtual void OnButtonClicked(ClickValueEventArgs<ModalWindowButtons> e)
        {
            ButtonClick?.Invoke(this, e);
        }
        protected virtual void OnButtonsChanged(ModalWindowButtons oldValue, ModalWindowButtons newValue)
        {
            if (newValue == ModalWindowButtons.All)
            {
                SetGridValues(_mainButton, 0, 1);
                SetGridValues(_secondaryButton, 2, 1);
                return;
            }
            else if (newValue == ModalWindowButtons.Main)
            {
                SetGridValues(_mainButton, 0, 3);
                _secondaryButton.Visibility = Visibility.Collapsed;
            }
            else if (newValue == ModalWindowButtons.Secondary)
            {
                SetGridValues(_secondaryButton, 0, 3);
                _mainButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnMainButtonClick(object sender, RoutedEventArgs e)
        {
            OnButtonClicked(new(ModalWindowButtons.Main, e));
        }
        private void OnSecondaryButtonClick(object sender, RoutedEventArgs e)
        {
            OnButtonClicked(new(ModalWindowButtons.Secondary, e));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _ignoreDrag = true;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            _ignoreDrag = false;
            _startDragWindowPosition = new(Left, Top);
            _startDragMousePosition = PointToScreen(e.GetPosition(this));
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed ||
                _ignoreDrag ||
                e.Handled ||
                !CanMove)
            {
                return;
            }

            var position = PointToScreen(e.GetPosition(this)) - _startDragMousePosition;
            Left = _startDragWindowPosition.X + position.X;
            Top = _startDragWindowPosition.Y + position.Y;
        }

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModalWindow view)
            {
                view._content.Content = e.NewValue;
            }
        }
        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModalWindow view)
            {
                view.OnButtonsChanged((ModalWindowButtons)e.OldValue, (ModalWindowButtons)e.NewValue);
            }
        }
        private static void OnMainButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModalWindow view)
            {
                view._mainButton.Content = e.NewValue;
            }
        }
        private static void OnSecondaryButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModalWindow view)
            {
                view._secondaryButton.Content = e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(object),
            typeof(ModalWindow), new(OnChildChanged));
        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(nameof(Buttons), typeof(ModalWindowButtons),
            typeof(ModalWindow), new(ModalWindowButtons.Main, OnButtonsChanged));
        public static readonly DependencyProperty MainButtonContentProperty = DependencyProperty.Register(nameof(MainButtonContent), typeof(object),
            typeof(ModalWindow), new(OnMainButtonContentChanged));
        public static readonly DependencyProperty SecondaryButtonContentProperty = DependencyProperty.Register(nameof(SecondaryButtonContent), typeof(object),
            typeof(ModalWindow), new(OnSecondaryButtonContentChanged));
        public static readonly DependencyProperty CanMoveProperty = DependencyProperty.Register(nameof(CanMove), typeof(bool),
            typeof(ModalWindow), new(true));

        #endregion
    }
}
