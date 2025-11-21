using System.ComponentModel;

namespace DialogMaker.Core
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region События

        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        protected void InvokePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        #endregion
    }
}
