using DialogMaker.Core.Executioning.Internal;
using System.ComponentModel;

namespace DialogMaker.Core.Executioning
{
    internal class DialogExecutorThreadManager : Disposable, IDialogExecutingThreadManager, IInternalDialogExecutor
    {
        public DialogExecutorThreadManager(DialogExecutor executor, IDialogExecutionResources resources, DialogByteCodeData data)
        {
            DialogExecutor = executor;
            Resources = resources;
            _data = data;
            _dialogHandler = new InternalDialogHandler(this);

            _threads.ItemChanged += OnThreadsItemChanged;
        }

        public event EventHandler<DialogExecutorHandleEventArgs>? DialogHandled;

        public DialogExecutor DialogExecutor { get; }
        public IDialogExecutionResources Resources { get; }
        public bool IsRunning
        {
            get;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(IsRunning));
                    field = value;

                    if (value)
                    {
                        _dialogHandler.OnDialogExecutingStarted(this, new(DialogExecutor));
                    }
                    else
                    {
                        _dialogHandler.OnDialogExecutingEnded(this, new(DialogExecutor));
                    }

                    OnPropertyChanged(nameof(IsRunning));
                }
            }
        }
        public bool IsPaused
        {
            get;
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
        IDialogExecutor IDialogExecutingThreadManager.DialogExecutor => DialogExecutor;

        private IDialogExecutionResources CurrentResources
        {
            get
            {
                if (_specialResources == null)
                {
                    return Resources;
                }

                return _specialResources;
            }
        }


        private readonly Dictionary<string, List<DialogExecutionJoinController>> _joinControllers = [];
        private readonly Dictionary<string, List<DialogExecutionIntersectController>> _intersectControllers = [];
        private readonly IDialogExecutingHandler _dialogHandler;
        private readonly EditableCollection<DialogThread> _threads = [];
        private readonly DialogByteCodeData _data;
        private readonly Stack<OperandValue> _stack = [];
        private IDialogExecutionResources? _specialResources;
        private HashSet<DialogThread>? _completingThreads;
        private bool _isStopping;

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
            if (_isStopping &&
                _completingThreads != null &&
                _completingThreads.Count > 0)
            {
                return;
            }

            StartThreadWithEvent(position, null);
        }
        public void StartThread(IDialogExecutionThread source, DialogPosition position)
        {
            if (_isStopping && !IsCompletingThread(source))
            {
                return;
            }
            if (source is DialogThread dialogThread)
            {
                _completingThreads?.Add(dialogThread);
            }

            StartThreadWithEvent(position, source);
        }
        public async void Stop()
        {
            await StopAsync();
        }
        public async Task StopAsync()
        {
            if (_isStopping || _threads.Count == 0)
            {
                return;
            }

            _isStopping = true;

            if (!IsDisposed)
            {
                var completingEventThread = StartEventSections(DialogExecutionEvent.Completing);

                if (completingEventThread != null && completingEventThread.Count > 0)
                {
                    _completingThreads = completingEventThread;

                    await Task.Run(() =>
                    {
                        while (true)
                        {
                            bool allCompleted = true;

                            foreach (var thread in completingEventThread)
                            {
                                if (thread.IsRunning)
                                {
                                    allCompleted = false;
                                }
                            }

                            if (allCompleted)
                            {
                                break;
                            }
                        }
                    });
                }

                _completingThreads = null;
            }

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
            IsRunning = false;
            _isStopping = false;
        }
        public void Pause()
        {
            if (IsPaused || _isStopping)
            {
                return;
            }

            SetIsPauseValue(true);
            StartEventSections(DialogExecutionEvent.Paused);
        }
        public void Resume()
        {
            if (!IsPaused || _isStopping)
            {
                return;
            }

            SetIsPauseValue(false);
            StartEventSections(DialogExecutionEvent.Resumed);
        }

        public async Task Reset(IDialogExecutionResources? newResources = null)
        {
            if (IsRunning)
            {
                await StopAsync();
            }

            _specialResources = newResources;
            _stack.Clear();
            CurrentResources.Reset();
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
            var resources = CurrentResources;

            foreach (var thread in _threads)
            {
                if (!thread.IsRunning && thread.Resources == resources)
                {
                    return thread;
                }
            }

            DialogThread newThread = new(this, _stack, resources, _dialogHandler, _data);
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

        private async void StartThreadWithEvent(DialogPosition position, IDialogExecutionThread? source)
        {
            if (!IsRunning)
            {
                StartEventSections(DialogExecutionEvent.Started);
            }

            StartThread(position, source);
        }
        private DialogThread StartThread(DialogPosition position, IDialogExecutionThread? source)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException("Невозможно запустить поток, так менеджер потоков был очищен");
            }

            var thread = GetOrCreateThread();
            StartThreadSync(thread, position, source);

            return thread;
        }
        private HashSet<DialogThread>? StartEventSections(DialogExecutionEvent executionEvent, IDialogExecutionThread? source = null)
        {
            HashSet<DialogThread>? startedThreads = null;

            if (_data.Metadata.EventSections.TryGetValue(executionEvent, out var sections))
            {
                startedThreads = new(sections.Count);

                foreach (var section in sections)
                {
                    var thread = StartThread(new(section), source);
                    startedThreads.Add(thread);
                }
            }

            return startedThreads;
        }

        private async void StartThreadSync(DialogThread thread, DialogPosition position, IDialogExecutionThread? source)
        {
            await thread.Start(position.Section, position.Operation, source);
        }
        private bool IsCompletingThread(IDialogExecutionThread thread)
        {
            var threads = _completingThreads;

            if (threads == null)
            {
                return false;
            }

            foreach (var t in threads)
            {
                if (t == thread)
                {
                    return true;
                }
            }

            return false;
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

            Stop();
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
