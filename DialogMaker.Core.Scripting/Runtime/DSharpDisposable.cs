namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Base class of disposable objects
    /// </summary>
    public abstract class DSharpDisposable : IDisposable
    {
        ~DSharpDisposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Event that invokes when object start disposing
        /// </summary>
        public event EventHandler? Disposed;

        /// <summary>
        /// Is current object has been disposed
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set
            {
                if (field != value)
                {
                    field = value;

                    if (value)
                    {
                        Disposed?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        #region Controls

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose current object
        /// </summary>
        /// <param name="disposing">Is manual disposing</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
