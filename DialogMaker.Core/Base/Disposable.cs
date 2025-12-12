using System;

namespace DialogMaker.Core
{
    public class Disposable : ObservableObject, IDisposable
    {
        ~Disposable()
        {
            Dispose(false);
        }

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

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
    }
}
