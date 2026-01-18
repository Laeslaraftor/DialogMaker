using System;
using System.Collections.Generic;

namespace DialogMaker.Core
{
    public class Disposable : ObservableObject, IDisposable
    {
        ~Disposable()
        {
            StartDisposing(false);
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

            StartDisposing(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            IsDisposed = true;

            Dispatch(() =>
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            });
        }

        private void StartDisposing(bool isDisposing)
        {
            Dispatch(() => Dispose(isDisposing));
        }

        #region Статика

        public static void DisposeAll(IEnumerable<IDisposable> disposables)
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        #endregion
    }
}
