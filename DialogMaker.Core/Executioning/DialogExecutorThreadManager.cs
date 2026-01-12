using Acly;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    internal class DialogExecutorThreadManager : Disposable, IDialogExecutingThreadManager, IInternalDialogExecutor
    {
        public DialogExecutorThreadManager(DialogExecutor executor, DialogByteCodeData data)
        {
            DialogExecutor = executor;
            _data = data;
            _dialogHandler = new InternalDialogHandler(this);

            _threads.ItemChanged += OnThreadsItemChanged;
        }

        public event EventHandler<DialogExecutorHandleEventArgs>? DialogHandled;

        public DialogExecutor DialogExecutor { get; }
        public bool IsRunning
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsRunning));
                    field = value;

                    if (value)
                    {
                        _dialogHandler.OnDialogExecutingStarted(this, EventArgs.Empty);
                    }
                    else
                    {
                        _dialogHandler.OnDialogExecutingEnded(this, EventArgs.Empty);
                    }

                    InvokePropertyChanged(nameof(IsRunning));
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
                    InvokePropertyChanging(nameof(IsPaused));
                    field = value;
                    InvokePropertyChanged(nameof(IsPaused));
                }
            }
        }

        private readonly Dictionary<string, List<DialogExecutionJoinController>> _joinControllers = [];
        private readonly Dictionary<string, List<DialogExecutionIntersectController>> _intersectControllers = [];
        private readonly IDialogExecutingHandler _dialogHandler;
        private readonly EditableCollection<DialogThread> _threads = [];
        private readonly DialogByteCodeData _data;
        private readonly Stack<OperandValue> _stack = [];

        #region Управление

        public DialogExecutionJoinController GetJoinController(DialogExecutionContext context, IJoinOperationInfo joinInfo)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException($"Невозможно получить контроллер объединения потоков, так как менеджер потоков был очищен");
            }

            return GetFreeController(_joinControllers, context, joinInfo, () => new(this, joinInfo));
        }
        public DialogExecutionIntersectController GetIntersectController(DialogExecutionContext context, IJoinOperationInfo joinInfo)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException($"Невозможно получить контроллер пересечения потоков, так как менеджер потоков был очищен");
            }
            
            return GetFreeController(_intersectControllers, context, joinInfo, () => new(this, joinInfo));
        }

        public void StartThread(DialogPosition position)
        {
            StartThread(position, null);
        }
        public void StartThread(IDialogExecutionThread source, DialogPosition position)
        {
            StartThread(position, source);
        }
        public async void Stop()
        {
            await StopAsync();
        }
        public async Task StopAsync()
        {
            foreach (var thread in _threads)
            {
                if (!thread.IsRunning)
                {
                    continue;
                }

                thread.IsPaused = false;
                thread.Stop();

                while (thread.IsRunning)
                {
                    await Task.Delay(10);
                }
            }

            IsPaused = false;
        }
        public void Pause() => SetIsPauseValue(true);
        public void Resume() => SetIsPauseValue(false);

        public async Task Reset()
        {
            if (IsRunning)
            {
                await StopAsync();
            }

            _stack.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Stop();

            foreach (var thread in _threads)
            {
                thread.Dispose();
            }
            foreach (var info in _joinControllers)
            {
                foreach (var controller in info.Value)
                {
                    controller.Dispose();
                }

                info.Value.Clear();
            }

            _joinControllers.Clear();
            _stack.Clear();
            _threads.Clear();
            _threads.ItemChanged -= OnThreadsItemChanged;
        }

        private DialogThread GetOrCreateThread()
        {
            foreach (var thread in _threads)
            {
                if (!thread.IsRunning)
                {
                    return thread;
                }
            }

            DialogThread newThread = new(this, _stack, DialogExecutor.Resources, _dialogHandler, _data);
            _threads.Add(newThread);

            return newThread;
        }
        private void SetIsPauseValue(bool value)
        {
            foreach (var thread in _threads)
            {
                thread.IsPaused = value;
            }

            IsPaused = value;
        }
        private async void StartThread(DialogPosition position, IDialogExecutionThread? source)
        {
            if (IsDisposed)
            {
                return;
            }

            var thread = GetOrCreateThread();
            await thread.Start(position.Section, position.Operation, source);
        }

        #endregion

        #region Обработка диалога

        public async Task HandleDialog(Func<IDialogExecutingHandler, Task> handler, CancellationToken cancellationToken)
        {
            bool isCompleted = false;

            InvokeDialogHandled(async h =>
            {
                await DialogExecutor.DispatchHandler<object?>(h, async h2 =>
                {
                    await handler(h2);
                    return null;
                });
                isCompleted = true;
            });

            while (!isCompleted && !cancellationToken.IsCancellationRequested)
            {
                await Task.DelaySafe(50, cancellationToken);
            }
        }
        public async Task<T?> HandleDialog<T>(Func<IDialogExecutingHandler, Task<T>> handler, CancellationToken cancellationToken, T? defaultValue = default)
        {
            bool isCompleted = false;
            T? result = defaultValue;

            InvokeDialogHandled(async h =>
            {
                result = await DialogExecutor.DispatchHandler(h, handler);
                isCompleted = true;
            });

            while (!isCompleted && !cancellationToken.IsCancellationRequested)
            {
                await Task.DelaySafe(50, cancellationToken);
            }

            return result;
        }
        public void InvokeDialogHandled(Action<IDialogExecutingHandler> handler)
        {
            var mainHandler = DialogExecutor.Handler;

            if (mainHandler != null)
            {
                handler(mainHandler);
            }

            DialogHandled?.Invoke(this, new(DialogExecutor, handler));
        }

        #endregion

        #region События

        private void OnThreadsItemChanged(object sender, CollectionItemEventArgs<DialogThread> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                e.Item.PropertyChanged += OnThreadPropertyChanged;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.PropertyChanged -= OnThreadPropertyChanged;
            }
        }

        private void OnThreadPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not DialogThread thread ||
                e.PropertyName != nameof(IsRunning))
            {
                return;
            }
            if (thread.IsRunning)
            {
                IsRunning = true;
                return;
            }
            foreach (var t in _threads)
            {
                if (t.IsRunning)
                {
                    IsRunning = true;
                    return;
                }
            }

            IsRunning = false;
        }

        #endregion

        #region Статика

        private static T GetFreeController<T>(Dictionary<string, List<T>> infos, DialogExecutionContext context, IJoinOperationInfo joinInfo, Func<T> fabric)
            where T : IJoinController
        {
            if (!infos.TryGetValue(joinInfo.Id, out var controllers))
            {
                controllers = [];
                infos.Add(joinInfo.Id, controllers);
            }

            T? freeController = default;

            foreach (var controller in controllers)
            {
                if (controller.CanJoin(context))
                {
                    if (controller.IsCompleted)
                    {
                        controller.Clear();
                    }

                    freeController = controller;
                    break;
                }
            }

            if (freeController == null)
            {
                freeController = fabric();
                controllers.Add(freeController);
            }

            return freeController;
        }

        #endregion
    }
}
