using DialogMaker.Core.Common;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class EmotionPreview : UserControl
    {
        public EmotionPreview()
        {
            InitializeComponent();
        }

        public IEmotion? Emotion
        {
            get => GetValue(EmotionProperty) as IEmotion;
            set => SetValue(EmotionProperty, value);
        }

        #region Управление

        private void SetEmotion(IEmotion? newValue)
        {
            _faceView.DataContext = newValue;
            _editButton.IsEnabled = newValue != null;
        }

        #endregion

        #region События

        private void OnEditButtonClicked(object sender, RoutedEventArgs e)
        {
            var emotion = Emotion;

            if (emotion != null)
            {
                FaceEditorView.Open(emotion);
            }
        }

        private static void OnEmotionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmotionPreview view)
            {
                view.SetEmotion(e.NewValue as IEmotion);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty EmotionProperty = DependencyProperty.Register(nameof(Emotion), typeof(IEmotion),
            typeof(EmotionPreview), new(OnEmotionChanged));

        #endregion
    }
}
