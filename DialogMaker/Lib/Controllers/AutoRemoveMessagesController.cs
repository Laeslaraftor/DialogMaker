using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor.Messages;
using DialogMaker.Lib.Elements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DialogMaker.Lib.Controllers
{
    public class AutoRemoveMessagesController : Disposable
    {
        public AutoRemoveMessagesController(Panel container)
        {
            Container = container;
            HoldDuration = TimeSpan.FromSeconds(4);
            FadeDuration = TimeSpan.FromSeconds(0.2);

            Messages.ItemChanged += OnMessagesItemChanged;
        }

        public Panel Container { get; }
        public EditableCollection<Message> Messages { get; } = [];
        public TimeSpan HoldDuration
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(HoldDuration));
                    field = value;
                    InvokePropertyChanged(nameof(HoldDuration));
                }
            }
        }
        public TimeSpan FadeDuration
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(FadeDuration));
                    field = value;
                    InvokePropertyChanged(nameof(FadeDuration));
                }
            }
        }
        public Thickness MessagesMargin
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(MessagesMargin));
                    field = value;

                    foreach (var view in _addedMessages.Keys)
                    {
                        view.Margin = MessagesMargin;
                    }

                    InvokePropertyChanged(nameof(MessagesMargin));
                }
            }
        }

        private readonly Dictionary<MessageView, Message> _addedMessages = [];
        private readonly ElementsPool<MessageView> _viewsPool = new();

        #region Управление

        private async void AddView(MessageView view)
        {
            if (!_addedMessages.ContainsKey(view))
            {
                return;
            }

            view.RemoveRequested -= OnMessageViewRemoveRequested;
            view.RemoveRequested += OnMessageViewRemoveRequested;

            view.Opacity = 0;
            Container.Children.Add(view);

            while (!view.IsLoaded)
            {
                await Task.Delay(10);
            }

            view.StartAutoRemoveTimer(HoldDuration);

            var currentMargin = MessagesMargin;
            var offsetMargin = GetOffsetMargin(view, currentMargin);

            AnimationsHelper.FadeIn(view, FadeDuration, HandoffBehavior.Compose);
            AnimationsHelper.AnimateMargin(view, offsetMargin, currentMargin, FadeDuration, HandoffBehavior.Compose);
            view.TranslateX(-Container.RenderSize.Width, 0, FadeDuration);
        }
        private async void RemoveView(MessageView view)
        {
            if (!_addedMessages.ContainsKey(view))
            {
                return;
            }

            view.RemoveRequested -= OnMessageViewRemoveRequested;
            view.CancelAutoRemoveTimer();
            _addedMessages.Remove(view);

            var currentMargin = view.Margin;
            var offsetMargin = GetOffsetMargin(view, currentMargin);

            AnimationsHelper.FadeOut(view, FadeDuration, HandoffBehavior.Compose);
            AnimationsHelper.AnimateMargin(view, currentMargin, offsetMargin, FadeDuration, HandoffBehavior.Compose);
            view.TranslateX(-Container.RenderSize.Width, FadeDuration);

            await Task.Delay(FadeDuration);

            Container.Children.Remove(view);
            _viewsPool.Free(view);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Messages.ItemChanged -= OnMessagesItemChanged;

            foreach (var view in _addedMessages.Keys)
            {
                view.RemoveRequested -= OnMessageViewRemoveRequested;
                Container.Children.Remove(view);
            }

            _addedMessages.Clear();
            _viewsPool.Dispose();
            Messages.Clear();
        }

        private Thickness GetOffsetMargin(MessageView view, Thickness currentMargin)
        {
            return new()
            {
                Left = currentMargin.Left,
                Top = -view.RenderSize.Height,
                Right = currentMargin.Right,
                Bottom = 0
            };
        }

        #endregion

        #region События

        private void OnMessageViewRemoveRequested(object? sender, EventArgs e)
        {
            if (sender is MessageView view)
            {
                RemoveView(view);
            }
        }

        private void OnMessagesItemChanged(object? sender, CollectionItemEventArgs<Message> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                var view = _viewsPool.GetElement();
                view.Message = e.Item;

                _addedMessages.Add(view, e.Item);

                AddView(view);
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                foreach (var info in _addedMessages)
                {
                    if (info.Value == e.Item)
                    {
                        RemoveView(info.Key);
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
