using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;

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

        public async void OnDialogExecutingEnded(object? sender, ItemEventArgs<IDialogExecutor> e)
        {
            await _executor.HandleDialog(h =>
            {
                h.OnDialogExecutingEnded(sender, e);
                return Task.CompletedTask;
            }, CancellationToken.None);
        }
        public async void OnDialogExecutingStarted(object? sender, ItemEventArgs<IDialogExecutor> e)
        {
            await _executor.HandleDialog(h =>
            {
                h.OnDialogExecutingStarted(sender, e);
                return Task.CompletedTask;
            }, CancellationToken.None);
        }

        #endregion
    }
}
