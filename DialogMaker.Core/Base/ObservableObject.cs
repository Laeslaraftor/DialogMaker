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
            OnPropertyChanging(propertyName);
            PropertyChanging?.Invoke(this, new(propertyName));
        }
        protected void InvokePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        #endregion
    }
}
