using DialogMaker.Lib.Elements;
using System.Windows;

namespace DialogMaker.Lib
{
    public static class Alerts
    {
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
        public static void Show(Exception error)
        {
            MessageBox.Show(error.Message, error.GetType().Name);
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
