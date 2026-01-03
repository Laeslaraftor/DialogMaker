using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using Acly;
using System.ComponentModel;

namespace DialogMaker.Core.Executioning
{
    public class DialogExecutor : Disposable
    {
        public DialogExecutor(byte[] code, ReadOnlyDictionary<int, int> sections, IDialogExecutionResources resources, IDialogExecutingHandler handler)
        {
            Resources = resources;
            Handler = handler;
            Sections = sections;
            _code = code;

            _threads.ItemChanged += OnThreadsItemChanged;
        }

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
                        Handler.OnDialogExecutingStarted(this, EventArgs.Empty);
                    }
                    else
                    {
                        Handler.OnDialogExecutingEnded(this, EventArgs.Empty);
                    }

                    InvokePropertyChanged(nameof(IsRunning));
                }
            }
        }
        public IDialogExecutionResources Resources { get; }
        public IDialogExecutingHandler Handler { get; }
        public ReadOnlyDictionary<int, int> Sections { get; }
        internal Stack<OperandValue> Stack { get; } = [];

        private readonly EditableCollection<DialogThread> _threads = [];
        private byte[] _code;

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
            await StartThread(0);
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

        internal async Task StartThread(int section)
        {
            var thread = GetOrCreateThread();
            await thread.Start(section);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Stop();

            _code = [];

            foreach (var thread in _threads)
            {
                thread.Dispose();
            }

            Stack.Clear();
            _threads.Clear();
            _threads.ItemChanged += OnThreadsItemChanged;
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

            DialogThread newThread = new(this, _code);
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
                    break;
                }
            }
        }

        #endregion
    }
}
