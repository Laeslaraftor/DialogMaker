using DialogMaker.Core;
using DialogMaker.Core.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DialogMaker.Lib.Elements
{
    public partial class MediaViewer : UserControl
    {
        public MediaViewer()
        {
            InitializeComponent();
        }

        public IResourceFile? MediaFile
        {
            get => GetValue(MediaFileProperty) as IResourceFile;
            set => SetValue(MediaFileProperty, value);
        }

        #region Управление

        private void SetMediaFile(IResourceFile? oldValue, IResourceFile? newValue)
        {
            _media.Close();
            _image.Visibility = Visibility.Collapsed;
            _media.Visibility = Visibility.Collapsed;
            _errorMessage.Visibility = Visibility.Collapsed;

            if (newValue == null)
            {
                return;
            }
            if (newValue.Type == DialogFileResourceType.Image)
            {
                try
                {
                    SetImage(newValue);
                }
                catch (Exception error)
                {
                    ShowError(error);
                }

                return;
            }

            _media.SetSource(newValue.FilePath);
            _media.Visibility = Visibility.Visible;
        }

        private void ShowError(Exception error)
        {
            _errorMessage.Text = $"{error.GetType().Name}: {error.Message}";
            _errorMessage.Visibility = Visibility.Visible;
        }
        private void SetImage(IResourceFile image)
        {
            BitmapImage bitmap = new(new(image.FilePath, UriKind.Absolute));
            _image.Source = bitmap;
            _image.Visibility = Visibility.Visible;
        }

        #endregion

        #region События

        private static void OnMediaFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaViewer view)
            {
                view.SetMediaFile(e.OldValue as IResourceFile, e.NewValue as IResourceFile);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty MediaFileProperty = DependencyProperty.Register(nameof(MediaFile), typeof(IResourceFile),
            typeof(MediaViewer), new(OnMediaFileChanged));

        #endregion
    }
}
