using DialogMaker.Core;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

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
