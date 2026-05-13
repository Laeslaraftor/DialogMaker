using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning.Internal
{
    internal class InternalDialogHandler(IInternalDialogExecutor executor) : IDialogExecutingHandler
    {
        public IDispatcher? Dispatcher { get; }

        private readonly IInternalDialogExecutor _executor = executor;

        public async Task HandleTrigger(Trigger trigger, DialogHandleEventArgs e)
        {
            await _executor.HandleDialog(async h =>
            {
                await h.HandleTrigger(trigger, e);
            }, e);
        }
        public async Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, DialogHandleEventArgs e)
        {
            await _executor.HandleDialog(async h =>
            {
                await h.ShowReplica(character, listener, text, e);
            }, e);
        }

        public async Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, DialogHandleEventArgs e)
        {
            return await _executor.HandleDialog(async h =>
            {
                return await h.ShowChoice(character, listener, variants, e);
            }, e);
        }
        public async Task ShowEmotion(ICharacter? character, IEmotion? emotion, DialogHandleEventArgs e)
        {
            await _executor.HandleDialog(async h =>
            {
                await h.ShowEmotion(character, emotion, e);
            }, e);
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

                dispatcher.Dispatch(() => action(h));
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
}
