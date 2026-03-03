using DialogMaker.Core.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning.Internal
{
    internal class InternalDialogHandler(IInternalDialogExecutor executor) : IDialogExecutingHandler
    {
        public IThreadDispatcher? Dispatcher { get; }

        private readonly IInternalDialogExecutor _executor = executor;

        public async Task HandleTrigger(Trigger trigger, CancellationToken cancellationToken)
        {
            await _executor.HandleDialog(async h =>
            {
                await h.HandleTrigger(trigger, cancellationToken);
            }, cancellationToken);
        }
        public async Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, CancellationToken cancellationToken)
        {
            await _executor.HandleDialog(async h =>
            {
                await h.ShowReplica(character, listener, text, cancellationToken);
            }, cancellationToken);
        }

        public async Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, CancellationToken cancellationToken)
        {
            return await _executor.HandleDialog(async h =>
            {
                return await h.ShowChoice(character, listener, variants, cancellationToken);
            }, cancellationToken);
        }
        public async Task ShowEmotion(ICharacter? character, IEmotion? emotion, CancellationToken cancellationToken)
        {
            await _executor.HandleDialog(async h =>
            {
                await h.ShowEmotion(character, emotion, cancellationToken);
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
}
