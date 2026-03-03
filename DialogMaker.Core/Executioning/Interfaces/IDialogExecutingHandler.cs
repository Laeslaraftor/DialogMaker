using DialogMaker.Core.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutingHandler
    {
        public IThreadDispatcher? Dispatcher { get; }

        public Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, CancellationToken cancellationToken);
        public Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, CancellationToken cancellationToken);
        public Task ShowEmotion(ICharacter? character, IEmotion? emotion, CancellationToken cancellationToken);

        public Task HandleTrigger(Trigger trigger, CancellationToken cancellationToken);

        public void OnDialogExecutingStarted(object? sender, EventArgs e);
        public void OnDialogExecutingEnded(object? sender, EventArgs e);
    }
}
