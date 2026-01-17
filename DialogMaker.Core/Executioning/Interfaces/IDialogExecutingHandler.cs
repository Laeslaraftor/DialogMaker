using DialogMaker.Core.Common;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutingHandler
    {
        public IThreadDispatcher? Dispatcher { get; }

        public Task ShowReplica(ICharacter? character, IResourceString text, CancellationToken cancellationToken);
        //public Task ShowFullscreenReplica(ICharacter? character, IResourceItem? background, IResourceString text, CancellationToken cancellationToken);
        //public Task ShowColorReplica(ICharacter? character, Color backgroundColor, Color textColor, IResourceString text, CancellationToken cancellationToken);
        public Task<int> ShowChoice(ICharacter? character, IStringCollection variants, CancellationToken cancellationToken);

        public Task HandleTrigger(string name, CancellationToken cancellationToken);

        public void OnDialogExecutingStarted(object? sender, EventArgs e);
        public void OnDialogExecutingEnded(object? sender, EventArgs e);
    }
}
