using DialogMaker.Core.Executioning.Internal;
using System.ComponentModel;

namespace DialogMaker.Core.Executioning
{
    public class DialogExecutor : Disposable
    {
        public DialogExecutor(byte[] code, IDialogExecutionResources resources)
            : this(DialogByteCodeData.Read(code), resources)
        {
        }
        public DialogExecutor(DialogByteCodeData data, IDialogExecutionResources resources)
        {
            Resources = resources;
            _resources = new(data, resources);
            _isolatedResources = new(data, resources);
            _threadManager = new(this, _resources, data);

            _threadManager.DialogHandled += OnThreadManagerDialogHandled;
            _threadManager.PropertyChanged += OnThreadManagerPropertyChanged;
        }

        public event EventHandler<DialogExecutorHandleEventArgs>? DialogHandled;

        public bool IsRunning
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(IsRunning));
                    field = value;
                    OnPropertyChanged(nameof(IsRunning));
                }
            }
        }
        public bool IsPaused
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(IsPaused));
                    field = value;
                    OnPropertyChanged(nameof(IsPaused));
                }
            }
        }
        public IDialogExecutionResources Resources { get; }
        public IDialogExecutingHandler? Handler
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Handler));
                    field = value;
                    OnPropertyChanged(nameof(Handler));
                }
            }
        }

        private readonly DialogExecutorThreadManager _threadManager;
        private readonly InternalDialogResources _resources;
        private readonly IsolatedDialogResources _isolatedResources;


        #region Управление

        public void PushIsolatedVariablesToResources()
        {
            _isolatedResources.PushGlobalVariables();
        }

        public async void Start(bool isolated)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException("Невозможно начать выполнение диалога, так как объект был очищен");
            }
            if (IsRunning)
            {
                return;
            }

            IDialogExecutionResources? newResources = null;

            if (isolated)
            {
                newResources = _isolatedResources;
            }

            await _threadManager.Reset(newResources);
            _threadManager.StartThread(new(0, 0));
        }
        public void Stop() => _threadManager.Stop();
        public void Pause() => _threadManager.Pause();
        public void Resume() => _threadManager.Resume();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (_threadManager == null)
            {
                return;
            }

            _threadManager.DialogHandled -= OnThreadManagerDialogHandled;
            _threadManager.PropertyChanged -= OnThreadManagerPropertyChanged;
            _threadManager.Dispose();
        }

        #endregion

        #region События

        private void OnThreadManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsRunning))
            {
                IsRunning = _threadManager.IsRunning;
            }
            else if (e.PropertyName == nameof(IsPaused))
            {
                IsPaused = _threadManager.IsPaused;
            }
        }
        private void OnThreadManagerDialogHandled(object sender, DialogExecutorHandleEventArgs e)
        {
            DialogHandled?.Invoke(this, e);
        }

        #endregion

        #region Статика

        internal static async Task DispatchHandler(IDialogExecutingHandler dialogHandler, Func<IDialogExecutingHandler, Task> handler)
        {
            await DispatchHandler<object?>(dialogHandler, async h =>
            {
                await handler(h);
                return null;
            });
        }
        internal static async Task<T?> DispatchHandler<T>(IDialogExecutingHandler dialogHandler, Func<IDialogExecutingHandler, Task<T>> handler)
        {
            var dispatcher = dialogHandler.Dispatcher;

            if (dispatcher == null)
            {
                return await handler(dialogHandler);
            }

            bool isCompleted = false;
            T? result = default;

            dispatcher.Dispatch(async () =>
            {
                result = await handler(dialogHandler);
                isCompleted = true;
            });

            while (!isCompleted)
            {
                await Task.Delay(10);
            }

            return result;
        }

        #endregion
    }
}
