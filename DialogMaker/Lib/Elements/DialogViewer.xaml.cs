using DialogMaker.Core;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogViewer : UserControl, IDialogExecutingHandler
    {
        public DialogViewer()
        {
            InitializeComponent();
        }

        IThreadDispatcher? IDialogExecutingHandler.Dispatcher => ObservableObject.Dispatcher;

        private readonly ElementsPool<DragView> _blocksPool = new();
        private readonly ElementsPool<TextBlock> _textPool = new();
        private readonly ElementsPool<DialogChoice> _dialogChoice = new();

        #region Управление

        public void Clear()
        {
            _history.Children.Clear();
            _textPool.Clear();
            _dialogChoice.Clear();
            _blocksPool.Clear();
        }

        public async Task ShowReplica(ICharacter? character, ICharacter? listener, IResourceString text, CancellationToken cancellationToken)
        {
            AddElement(character, listener, text.Text);
            await Task.DelaySafe(200, cancellationToken);
        }
        public async Task ShowEmotion(ICharacter? character, IEmotion? emotion, CancellationToken cancellationToken)
        {
            AddElement(null, null, $"{character?.Name} показывает какую то эмоцию");
            await Task.DelaySafe(200, cancellationToken);
        }
        //public async Task ShowColorReplica(ICharacter? character, SystemColor backgroundColor, SystemColor textColor, IResourceString text, CancellationToken cancellationToken)
        //{
        //    await ShowReplica(character, text, cancellationToken);
        //}
        //public async Task ShowFullscreenReplica(ICharacter? character, IResourceItem? background, IResourceString text, CancellationToken cancellationToken)
        //{
        //    await ShowReplica(character, text, cancellationToken);
        //}

        public async Task<int> ShowChoice(ICharacter? character, ICharacter? listener, IStringCollection variants, CancellationToken cancellationToken)
        {
            bool isCompleted = false;
            int result = -1;
            var block = _dialogChoice.GetElement();
            block.RemoveFromParent();
            block.IsEnabled = true;
            block.SelectedIndex = -1;
            block.Character = character;
            block.Variants = variants.Strings;

            void OnBlockChoiceChanged(object? sender, ValueChangedEventArgs<int> e)
            {
                isCompleted = true;
                result = e.NewValue;
                block.IsEnabled = false;

                block.ChoiceChanged -= OnBlockChoiceChanged;
            }

            block.ChoiceChanged -= OnBlockChoiceChanged;
            block.ChoiceChanged += OnBlockChoiceChanged;

            var container = GetNewBlock();
            container.Preview = block;

            container.BringIntoView();

            while (!isCompleted && !cancellationToken.IsCancellationRequested)
            {
                await Task.DelaySafe(50, cancellationToken);
            }

            block.ChoiceChanged -= OnBlockChoiceChanged;

            return result;
        }

        public async Task HandleTrigger(string name, CancellationToken cancellationToken)
        {
            AddElement(null, null, $"Событие: {name}");
        }

        private void AddElement(ICharacter? character, ICharacter? listener, string text)
        {
            var block = _textPool.GetElement();
            block.TextWrapping = TextWrapping.Wrap;
            block.RemoveFromParent();
                
            var container = GetNewBlock();
            container.Preview = block;
            string? characterText = GetActionText(character, listener, "обращается к");

            if (!string.IsNullOrEmpty(characterText))
            {
                block.Text = $"{characterText}: {text}";
                block.Opacity = 1;
            }
            else
            {
                block.Text = text;
                block.Opacity = 0.5;
            }

            container.BringIntoView();
        }
        private DragView GetNewBlock()
        {
            var result = _blocksPool.GetElement();
            result.RemoveFromParent();
            _history.Children.Add(result);

            return result;
        }
        private string? GetActionText(ICharacter? character, ICharacter? listener, string action)
        {
            if (character != null && listener != null)
            {
                return $"{character.Name} {action} {listener.Name}";
            }
            else if (character != null)
            {
                return character.Name;
            }
            else if (listener != null)
            {
                return $"{listener.Name} слушает";
            }

            return null;
        }

        #endregion

        #region События

        public void OnDialogExecutingEnded(object? sender, EventArgs e)
        {
            AddElement(null, null, "Диалог завершён");
        }
        public void OnDialogExecutingStarted(object? sender, EventArgs e)
        {
            AddElement(null, null, "Диалог начат");
        }

        #endregion
    }
}
