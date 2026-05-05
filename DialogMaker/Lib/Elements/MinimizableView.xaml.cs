using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class MinimizableView : UserControl
    {
        public MinimizableView()
        {
            InitializeComponent();
        }

        public event EventHandler<ValueChangedEventArgs<bool>>? IsMinimizedChanged;

        public bool IsMinimized
        {
            get => (bool)GetValue(IsMinimizedProperty);
            set => SetValue(IsMinimizedProperty, value);
        }
        public UIElement? Header
        {
            get => GetValue(HeaderProperty) as UIElement;
            set => SetValue(HeaderProperty, value);
        }
        public UIElement? Child
        {
            get => GetValue(ChildProperty) as UIElement;
            set => SetValue(ChildProperty, value);
        }

        #region Управление

        private void SetMinimized(bool value)
        {
            double angle = 0;
            Visibility visibility = Visibility.Visible;

            if (value)
            {
                angle = -90;
                visibility = Visibility.Collapsed;
            }

            _minimizeButtonRotation.Angle = angle;
            _heightBorder.Visibility = visibility;
            _contentContainer.Visibility = visibility;
        }

        #endregion

        #region События

        private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
        {
            IsMinimized = !IsMinimized;
        }

        private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MinimizableView view)
            {
                view.SetMinimized((bool)e.NewValue);
                view.IsMinimizedChanged?.Invoke(view, new((bool)e.OldValue, (bool)e.NewValue));
            }
        }
        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MinimizableView view)
            {
                view._headerContainer.Child = e.NewValue as UIElement;
            }
        }
        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MinimizableView view)
            {
                view._contentContainer.Child = e.NewValue as UIElement;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register(nameof(IsMinimized), typeof(bool),
            typeof(MinimizableView), new(OnIsMinimizedChanged));
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(UIElement),
            typeof(MinimizableView), new(OnHeaderChanged));
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(UIElement),
            typeof(MinimizableView), new(OnChildChanged));

        #endregion
    }
}
