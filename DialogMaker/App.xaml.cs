using DialogMaker.Core;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace DialogMaker
{
    public partial class App : Application, IThreadDispatcher
    {
        public bool CurrentThreadIsMain => Dispatcher.Thread == Thread.CurrentThread;

        #region Управление

        public void Execute(Action action)
        {
            Dispatcher.Invoke(action);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Disposable.Dispatcher = this;
        }

        #endregion

        #region Статика

        public static Brush TextBrush
        {
            get
            {
                if (field == null)
                {
                    if (!TryFindResource<Brush>("TextFillColorPrimaryBrush", out var brush))
                    {
                        brush = Brushes.Black;
                    }

                    field = brush;
                }

                return field;
            }
        }
        public static Brush SuccessBrush
        {
            get
            {
                if (field == null)
                {
                    if (!TryFindResource<Brush>("SystemFillColorSuccessBrush", out var brush))
                    {
                        brush = new SolidColorBrush(Color.FromArgb(255, 50, 167, 81));
                    }

                    field = brush;
                }

                return field;
            }
        }
        public static Brush WarningBrush
        {
            get
            {
                if (field == null)
                {
                    if (!TryFindResource<Brush>("SystemFillColorCautionBrush", out var brush))
                    {
                        brush = new SolidColorBrush(Color.FromArgb(255, 50, 167, 81));
                    }

                    field = brush;
                }

                return field;
            }
        }
        public static Brush ErrorBrush
        {
            get
            {
                if (field == null)
                {
                    if (!TryFindResource<Brush>("SystemFillColorCriticalBrush", out var brush))
                    {
                        brush = new SolidColorBrush(Color.FromArgb(255, 50, 167, 81));
                    }

                    field = brush;
                }

                return field;
            }
        }

        public static bool TryFindResource<T>(string name, [NotNullWhen(true)] out T? result)
        {
            result = default;

            if (Current == null)
            {
                return false;
            }

            var resource = Current.TryFindResource(name);

            if (resource is T typedResource)
            {
                result = typedResource;
            }

            return result != null;
        }

        #endregion
    }

}
