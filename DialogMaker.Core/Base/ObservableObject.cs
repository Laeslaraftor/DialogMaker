using System.ComponentModel;

namespace DialogMaker.Core
{
    public class ObservableObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

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
            OnPropertyChanging(args.PropertyName);
            PropertyChanging?.Invoke(this, args);
        }
        protected void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void InvokePropertyChanged(PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args.PropertyName);
            PropertyChanged?.Invoke(this, args);
        }

        #endregion
    }
}
