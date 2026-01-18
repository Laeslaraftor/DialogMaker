using System;
using System.ComponentModel;

namespace DialogMaker.Core
{
    public class ObservableObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        #region Управление

        protected void Dispatch(Action action)
        {
            var dispatcher = Dispatcher;

            if (dispatcher != null && !dispatcher.CurrentThreadIsMain)
            {
                dispatcher.Execute(action);
                return;
            }

            action();
        }

        #endregion

        #region События

        protected virtual void OnPropertyChanging(string propertyName)
        {
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        protected void InvokePropertyChanging(string propertyName)
        {
            InvokePropertyChanging(new PropertyChangingEventArgs(propertyName));
        }
        protected void InvokePropertyChanging(PropertyChangingEventArgs args)
        {
            Dispatch(() =>
            {
                OnPropertyChanging(args.PropertyName);
                PropertyChanging?.Invoke(this, args);
            });
        }
        protected void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void InvokePropertyChanged(PropertyChangedEventArgs args)
        {
            Dispatch(() =>
            {
                OnPropertyChanged(args.PropertyName);
                PropertyChanged?.Invoke(this, args);
            });
        }

        #endregion

        #region Статика

        public static IThreadDispatcher? Dispatcher { get; set; }

        #endregion
    }
}
