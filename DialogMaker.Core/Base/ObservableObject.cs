using System;
using System.Collections.Generic;
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
            if (!_propertyChangingEventArgs.TryGetValue(propertyName, out var args))
            {
                args = new(propertyName);
                _propertyChangingEventArgs.Add(propertyName, args);
            }

            InvokePropertyChanging(args);
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
            if (!_propertyChangedEventArgs.TryGetValue(propertyName, out var args))
            {
                args = new(propertyName);
                _propertyChangedEventArgs.Add(propertyName, args);
            }

            InvokePropertyChanged(args);
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

        private static readonly Dictionary<string, PropertyChangedEventArgs> _propertyChangedEventArgs = [];
        private static readonly Dictionary<string, PropertyChangingEventArgs> _propertyChangingEventArgs = [];

        #endregion
    }
}
