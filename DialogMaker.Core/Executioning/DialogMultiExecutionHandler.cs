using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public class DialogMultiExecutionHandler : Disposable
    {
        public DialogMultiExecutionHandler()
        {
            _relayHandler = new(this);
        }

        public event EventHandler<ItemEventArgs<IDialogExecutor>>? DialogStarted;
        public event EventHandler<ItemEventArgs<IDialogExecutor>>? DialogEnded;

        public EditableCollection<IDialogExecutingHandler> DialogHanders { get; } = [];

        private readonly HashSet<IDialogExecutor> _executors = [];
        private readonly RelayHandler _relayHandler;

        #region Управление

        public bool AddExecutor(IDialogExecutor executor)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ObjectDisposedExceptionMessage);
            }
            if (_executors.Add(executor))
            {
                executor.DialogHandled += OnExecutorDialogHandled;
                executor.Disposed += OnExecutorDisposed;
                return true;
            }

            return false;
        }
        public bool RemoveExecutor(IDialogExecutor executor)
        {
            if (_executors.Remove(executor))
            {
                Clear(executor);
                return true;
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var executor in _executors)
            {
                Clear(executor);
            }

            _executors.Clear();
        }

        private void Clear(IDialogExecutor dialogExecutor)
        {
            dialogExecutor.DialogHandled -= OnExecutorDialogHandled;
            dialogExecutor.Disposed -= OnExecutorDisposed;
        }

        #endregion

        #region Обработка диалога

        protected virtual async Task HandleTrigger(Trigger trigger, DialogHandleEventArgs e)
        {
            foreach (var handler in DialogHanders)
            {
                await handler.HandleTrigger(trigger, e);
            }
        }
        protected virtual async Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, DialogHandleEventArgs e)
        {
            int maxValue = -1;

            foreach (var handler in DialogHanders)
            {
                var answer = await handler.ShowChoice(character, listener, variants, e);
                maxValue = Math.Max(maxValue, answer);
            }

            return maxValue;
        }
        protected virtual async Task ShowEmotion(ICharacter? character, IEmotion? emotion, DialogHandleEventArgs e)
        {
            foreach (var handler in DialogHanders)
            {
                await handler.ShowEmotion(character, emotion, e);
            }
        }
        protected virtual async Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, DialogHandleEventArgs e)
        {
            foreach (var handler in DialogHanders)
            {
                await handler.ShowReplica(character, listener, text, e);
            }
        }

        #endregion

        #region События

        protected virtual void OnDialogExecutingStarted(object? sender, ItemEventArgs<IDialogExecutor> e)
        {
            foreach (var handler in DialogHanders)
            {
                handler.OnDialogExecutingStarted(sender, e);
            }

            DialogStarted?.Invoke(this, e);
        }
        protected virtual void OnDialogExecutingEnded(object? sender, ItemEventArgs<IDialogExecutor> e)
        {
            foreach (var handler in DialogHanders)
            {
                handler.OnDialogExecutingEnded(sender, e);
            }

            DialogEnded?.Invoke(this, e);
        }

        protected virtual void OnExecutorDialogHandled(object sender, DialogExecutorHandleEventArgs e)
        {
            e.Handle(_relayHandler);
        }
        private void OnExecutorDisposed(object sender, EventArgs e)
        {
            if (sender is DialogExecutor executor)
            {
                RemoveExecutor(executor);
            }
        }

        #endregion

        #region Классы

        private sealed class RelayHandler(DialogMultiExecutionHandler handler) : IDialogExecutingHandler
        {
            public IDispatcher? Dispatcher => _handler.Dispatcher;

            private readonly DialogMultiExecutionHandler _handler = handler;

            #region Управление

            public async Task HandleTrigger(Trigger trigger, DialogHandleEventArgs e)
            {
                await _handler.HandleTrigger(trigger, e);
            }
            public async Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, DialogHandleEventArgs e)
            {
                return await _handler.ShowChoice(character, listener, variants, e);
            }
            public async Task ShowEmotion(ICharacter? character, IEmotion? emotion, DialogHandleEventArgs e)
            {
                await _handler.ShowEmotion(character, emotion, e);
            }
            public async Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, DialogHandleEventArgs e)
            {
                await _handler.ShowReplica(character, listener, text, e);
            }

            #endregion

            #region События

            public void OnDialogExecutingStarted(object? sender, ItemEventArgs<IDialogExecutor> e)
            {
                _handler.OnDialogExecutingStarted(sender, e);
            }
            public void OnDialogExecutingEnded(object? sender, ItemEventArgs<IDialogExecutor> e)
            {
                _handler.OnDialogExecutingEnded(sender, e);
            }

            #endregion
        }

        #endregion
    }
}
