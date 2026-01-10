using Acly;
using DialogMaker.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
            _data = data;
            _internalHandler = new(this);

            _threads.ItemChanged += OnThreadsItemChanged;
        }

        public event EventHandler<DialogExecutorHandleEventArgs>? DialogHandled;

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
                        _internalHandler.OnDialogExecutingStarted(this, EventArgs.Empty);
                    }
                    else
                    {
                        _internalHandler.OnDialogExecutingEnded(this, EventArgs.Empty);
                    }

                    InvokePropertyChanged(nameof(IsRunning));
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
                    InvokePropertyChanging(nameof(Handler));
                    field = value;
                    InvokePropertyChanged(nameof(Handler));
                }
            }
        }
        internal Stack<OperandValue> Stack { get; } = [];

        private readonly EditableCollection<DialogThread> _threads = [];
        private readonly InternalHandler _internalHandler;
        private readonly DialogByteCodeData _data;

        #region Управление

        public async void Start()
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException("Невозможно начать выполнение диалога, так как объект был очищен");
            }
            if (IsRunning)
            {
                return;
            }

            Stack.Clear();
            await StartThread(0, 0);
        }

        public async Task StopAsync()
        {
            foreach (var thread in _threads)
            {
                if (!thread.IsRunning)
                {
                    continue;
                }

                thread.StopThread();

                while (thread.IsRunning)
                {
                    await Task.Delay(10);
                }
            }
        }
        public async void Stop()
        {
            await StopAsync();
        }

        internal async Task StartThread(int section, int instructionPosition)
        {
            var thread = GetOrCreateThread();
            await thread.Start(section, instructionPosition);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Stop();

            foreach (var thread in _threads)
            {
                thread.Dispose();
            }

            Stack.Clear();
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

            DialogThread newThread = new(this, _internalHandler, _data);
            _threads.Add(newThread);

            return newThread;
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

        private async Task HandleDialog(Func<IDialogExecutingHandler, Task> handler, CancellationToken cancellationToken)
        {
            bool isCompleted = false;

            InvokeDialogHandled(async h =>
            {
                await DispatchHandler<object?>(h, async h2 =>
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
        private async Task<T?> HandleDialog<T>(Func<IDialogExecutingHandler, Task<T>> handler, CancellationToken cancellationToken, T? defaultValue = default)
        {
            bool isCompleted = false;
            T? result = defaultValue;

            InvokeDialogHandled(async h =>
            {
                result = await DispatchHandler(h, handler);
                isCompleted = true;
            });

            while (!isCompleted && !cancellationToken.IsCancellationRequested)
            {
                await Task.DelaySafe(50, cancellationToken);
            }

            return result;
        }
        private void InvokeDialogHandled(Action<IDialogExecutingHandler> handler)
        {
            var mainHandler = Handler;

            if (mainHandler != null)
            {
                handler(mainHandler);
            }

            DialogHandled?.Invoke(this, new(this, handler));
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

            dispatcher.Execute(async () =>
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

        #region Классы

        private class InternalHandler(DialogExecutor executor) : IDialogExecutingHandler
        {
            public IThreadDispatcher? Dispatcher { get; }

            private readonly DialogExecutor _executor = executor;

            public async Task HandleTrigger(string name, CancellationToken cancellationToken)
            {
                await _executor.HandleDialog(async h =>
                {
                    await h.HandleTrigger(name, cancellationToken);
                }, cancellationToken);
            }

            public async Task ShowColorReplica(ICharacter? character, Color backgroundColor, Color textColor, IResourceString text, CancellationToken cancellationToken)
            {
                await _executor.HandleDialog(async h =>
                {
                    await h.ShowColorReplica(character, backgroundColor, textColor, text, cancellationToken);
                }, cancellationToken);
            }
            public async Task ShowFullscreenReplica(ICharacter? character, IResourceItem? background, IResourceString text, CancellationToken cancellationToken)
            {
                await _executor.HandleDialog(async h =>
                {
                    await h.ShowFullscreenReplica(character, background, text, cancellationToken);
                }, cancellationToken);
            }
            public async Task ShowReplica(ICharacter? character, IResourceString text, CancellationToken cancellationToken)
            {
                await _executor.HandleDialog(async h =>
                {
                    await h.ShowReplica(character, text, cancellationToken);
                }, cancellationToken);
            }

            public async Task<int> ShowChoice(ICharacter? character, IStringCollection variants, CancellationToken cancellationToken)
            {
                return await _executor.HandleDialog(async h =>
                {
                    return await h.ShowChoice(character, variants, cancellationToken);
                }, cancellationToken);
            }

            #region События

            private void InvokeDispatched(Action<IDialogExecutingHandler> action)
            {
                _executor.InvokeDialogHandled(h =>
                {
                    var dispatcher = h.Dispatcher;

                    if (dispatcher == null)
                    {
                        action(h);
                        return;
                    }

                    dispatcher.Execute(() => action(h));
                });
            }

            public void OnDialogExecutingEnded(object? sender, EventArgs e)
            {
                InvokeDispatched(h =>
                {
                    h.OnDialogExecutingEnded(sender, e);
                });
            }
            public void OnDialogExecutingStarted(object? sender, EventArgs e)
            {
                InvokeDispatched(h =>
                {
                    h.OnDialogExecutingStarted(sender, e);
                });
            }

            #endregion
        }

        #endregion
    }
}
