using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutingHandler
    {
        public IDispatcher? Dispatcher { get; }

        public Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, DialogHandleEventArgs e);
        public Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, DialogHandleEventArgs e);
        public Task ShowEmotion(ICharacter? character, IEmotion? emotion, DialogHandleEventArgs e);
        public Task HandleTrigger(Trigger trigger, DialogHandleEventArgs e);

        public void OnDialogExecutingStarted(object? sender, EventArgs e);
        public void OnDialogExecutingEnded(object? sender, EventArgs e);
    }
}
