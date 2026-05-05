using DialogMaker.Editor;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class ReplicaView : UserControl
    {
        public ReplicaView()
        {
            InitializeComponent();

            _idBlock.EditCommand = new RelayCommand(ExecuteChangeId, CanChangeId);
        }

        public ProjectString? ProjectString
        {
            get => GetValue(ProjectStringProperty) as ProjectString;
            set => SetValue(ProjectStringProperty, value);
        }
        public bool IsMinimized
        {
            get => (bool)GetValue(IsMinimizedProperty);
            set => SetValue(IsMinimizedProperty, value);
        }

        #region Управление

        private void SetIsMinimized(bool value)
        {
            double rotation = 0;
            Visibility elementsVisibility = Visibility.Visible;

            if (value)
            {
                rotation = -90;
                elementsVisibility = Visibility.Collapsed;
            }

            _minimizeButtonRotation.Angle = rotation;
            _preview.Visibility = _heightBorder.Visibility;
            _heightBorder.Visibility = elementsVisibility;
            _extraContainer.Visibility = elementsVisibility;
        }

        #endregion

        #region Команды

        private bool CanChangeId(object? parameter)
        {
            return ProjectString != null;
        }
        private void ExecuteChangeId(object? parameter)
        {
            var str = ProjectString;

            if (str == null ||
                parameter is not EditCommandEventArgs<string> args)
            {
                return;
            }

            str.Id = args.NewValue.Trim();
        }

        #endregion

        #region События

        private void OnProjectStringChanged(ProjectString? oldValue, ProjectString? newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnProjectStringPropertyChanged;
            }

            _idBlock.DataContext = newValue;
            _preview.DataContext = newValue?.PreviewVariant;
            _variantsList.ItemsSource = newValue?.Variants;
            _addVariantButton.IsEnabled = newValue != null;
            _addVariantButton.Command = newValue?.AddVariantCommand;
            _flagsView.Value = newValue?.Original.Resources.Flags;

            if (newValue != null)
            {
                newValue.PropertyChanged += OnProjectStringPropertyChanged;
            }
        }

        private void OnProjectStringPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PreviewVariant")
            {
                _preview.DataContext = ProjectString?.PreviewVariant;
            }
        }

        private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
        {
            IsMinimized = !IsMinimized;
        }

        private static void OnProjectStringPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReplicaView view)
            {
                view.OnProjectStringChanged(e.OldValue as ProjectString, e.NewValue as ProjectString);
            }
        }
        private static void OnIsMinimizedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReplicaView view)
            {
                view.SetIsMinimized((bool)e.NewValue);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ProjectStringProperty =
            DependencyProperty.Register(nameof(ProjectString), typeof(ProjectString), typeof(ReplicaView),
                new(OnProjectStringPropertyChanged));
        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register(nameof(IsMinimized), typeof(bool), typeof(ReplicaView),
                new(OnIsMinimizedPropertyChanged));

        #endregion
    }
}
