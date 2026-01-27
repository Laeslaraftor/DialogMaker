using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class EmotionView : UserControl
    {
        public EmotionView()
        {
            InitializeComponent();

            _idBlock.EditCommand = ProjectResourceItem.IdEditCommand;
        }

        public ProjectEmotion? Emotion
        {
            get => GetValue(EmotionProperty) as ProjectEmotion;
            set => SetValue(EmotionProperty, value);
        }

        #region События

        private void OnEmotionChanged(ProjectEmotion? oldValue, ProjectEmotion? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            _idBlock.DataContext = newValue;
            _idBlock.EditCommandParameter = newValue;
            _preview.Emotion = newValue?.Original;
            _flagsView.Value = newValue?.Original.Resources.Flags;
            ContextMenu = newValue?.ContextMenu;
        }

        private static void OnEmotionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmotionView view)
            {
                view.OnEmotionChanged(e.OldValue as ProjectEmotion, e.NewValue as ProjectEmotion);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty EmotionProperty = DependencyProperty.Register(nameof(Emotion), typeof(ProjectEmotion),
            typeof(EmotionView), new(OnEmotionChanged));

        #endregion
    }
}
