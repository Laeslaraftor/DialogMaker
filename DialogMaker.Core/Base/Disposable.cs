using System;

namespace DialogMaker.Core
{
    public class Disposable : ObservableObject, IDisposable
    {
        ~Disposable()
        {
            Dispose(false);
        }

        public event EventHandler? Disposed;

        public bool IsDisposed
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsDisposed));
                    field = value;
                    InvokePropertyChanged(nameof(IsDisposed));
                }
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Disposed?.Invoke(this, EventArgs.Empty);

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
    }
}
