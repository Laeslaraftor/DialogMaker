using DialogMaker.Core;
using DialogMaker.Core.Common;
using DialogMaker.Lib.Controllers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class FaceEditorView : UserControl
    {
        public FaceEditorView()
        {
            InitializeComponent();
            _propertiesList.ItemsSource = _properties;
        }

        public IEmotion? Emotion
        {
            get => GetValue(EmotionProperty) as IEmotion;
            set => SetValue(EmotionProperty, value);
        }

        private readonly ObservableCollection<PropertyEditorController> _properties = [];

        #region Управление

        private void SetEmotion(IEmotion? newValue)
        {
            _properties.Clear();
            _faceView.DataContext = newValue;

            if (newValue == null)
            {
                return;
            }

            void AddObject(object? obj)
            {
                if (obj is not ObservableObject observable)
                {
                    return;
                }

                var properties = PropertyEditorController.CreateForAllProperties(observable);
                
                foreach (var property in properties)
                {
                    _properties.Add(property);
                }
            }

            AddObject(newValue.LeftEye?.Eyebrow);
            AddObject(newValue.LeftEye);
            AddObject(newValue.RightEye?.Eyebrow);
            AddObject(newValue.RightEye);
            AddObject(newValue.Mouth);
        }

        #endregion

        #region События

        private static void OnEmotionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FaceEditorView view)
            {
                view.SetEmotion(e.NewValue as IEmotion);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty EmotionProperty = DependencyProperty.Register(nameof(Emotion), typeof(IEmotion),
            typeof(FaceEditorView), new(OnEmotionChanged));

        #endregion

        #region Статика

        private static readonly ElementsPool<FaceEditorView> _viewsPool = new();

        public static async void Open(IEmotion emotion)
        {
            await OpenAsync(emotion);
        }
        public static async Task OpenAsync(IEmotion emotion)
        {
            var view = _viewsPool.GetElement();
            view.Emotion = emotion;
            bool isClosed = false;

            Window window = new()
            {
                Title = "Редактор эмоции",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = view
            };

            window.Closed += OnWindowClosed;

            void OnWindowClosed(object? sender, EventArgs e)
            {
                window.Closed -= OnWindowClosed;
                window.Content = null;
                isClosed = true;
                view.Emotion = null;
                _viewsPool.Free(view);
            }

            window.Show();

            while (!isClosed)
            {
                await Task.Delay(50);
            }
        }

        #endregion
    }
}
