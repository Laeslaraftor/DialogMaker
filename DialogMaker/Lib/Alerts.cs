using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib
{
    public static class Alerts
    {
        private static readonly ElementsPool<AlertView> _alertViews = new();

        public static string? RequestText(string title, string confirmText = "Подтвердить")
        {
            var window = CreateDialogWindow(title);
            TextEntry entry = new();
            string? result = null;
            entry.Button.Content = confirmText;
            entry.Button.Click += OnButtonClick;

            void OnButtonClick(object sender, RoutedEventArgs e)
            {
                result = entry.TextBox.Text;
                window.Close();
            }

            window.Content = entry;

            window.ShowDialog();

            entry.Button.Click -= OnButtonClick;

            return result;
        }
        public static async Task<bool> Fill<T>(string title, T instance, IFillerValidator<T>? validator = null)
            where T : ObservableObject
        {
            bool isConfirmed = false;
            bool isCompleted = false;
            StackPanel inputsPanel = new()
            {
                CanVerticallyScroll = true
            };
            var alert = _alertViews.GetElement();
            alert.Title = title;
            alert.Message = inputsPanel;

            Dictionary<PropertyEditorController, object?> startValues = [];
            var properties = PropertyEditorController.CreateForAllProperties(instance);

            foreach (var property in properties)
            {
                startValues.Add(property, property.Value);

                var view = property.InputField.View;
                view.Margin = new(0, 2, 0, 2);

                inputsPanel.Children.Add(view);
            }

            ModalWindow window = new()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Buttons = ModalWindowButtons.All,
                MainButtonContent = "Продолжить",
                SecondaryButtonContent = "Отмена",
                SizeToContent = SizeToContent.Height,
                Width = 400,
                MaxHeight = 600,
                Child = alert,
                CanMove = false
            };

            window.ButtonClick += OnWindowButtonClicked;

            void OnWindowButtonClicked(object? sender, ClickValueEventArgs<ModalWindowButtons> e)
            {
                isConfirmed = e.Value == ModalWindowButtons.Main;

                if (isConfirmed && validator?.Validate(instance) != true)
                {
                    isConfirmed = false;
                    return;
                }

                window.ButtonClick -= OnWindowButtonClicked;

                if (e.Value == ModalWindowButtons.Secondary)
                {
                    foreach (var info in startValues)
                    {
                        info.Key.Value = info.Value;
                    }
                }

                isCompleted = true;

                window.Close();

                window.Child = null;
            }

            window.ShowDialog();

            while (!isCompleted)
            {
                await Task.Delay(50);
            }

            return isConfirmed;
        }

        public static void Show(Exception error)
        {
            ExceptionAlertView.Show(error);
        }
        public static void Show(string title, object? message)
        {
            var view = _alertViews.GetElement();
            view.Title = title;
            view.Message = message;

            ModalWindow window = new()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.Height,
                Buttons = ModalWindowButtons.Main,
                Width = 400,
                MainButtonContent = "Закрыть",
                Child = view
            };

            window.ButtonClick += OnWindowButtonClicked;

            void OnWindowButtonClicked(object? sender, ClickValueEventArgs<ModalWindowButtons> e)
            {
                window.Child = null;
                window.ButtonClick -= OnWindowButtonClicked;
                window.Close();
            }

            window.ShowDialog();

            view.Message = null;
            _alertViews.Free(view);
        }

        private static Window CreateDialogWindow(string title = "")
        {
            Window result = new()
            {
                Title = title,
                Width = 350,
                Height = 250,
                ResizeMode = ResizeMode.NoResize
            };

            return result;
        }
    }
}
