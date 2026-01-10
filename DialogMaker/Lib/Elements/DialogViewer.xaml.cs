using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using DialogMaker.Core;
using System.Windows;
using System.Windows.Controls;
using SystemColor = System.Drawing.Color;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogViewer : UserControl, IDialogExecutingHandler
    {
        public DialogViewer()
        {
            InitializeComponent();
        }

        IThreadDispatcher? IDialogExecutingHandler.Dispatcher => Disposable.Dispatcher;

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

        public async Task ShowReplica(ICharacter? character, IResourceString text, CancellationToken cancellationToken)
        {
            AddElement(character, text.Text);
            await Task.DelaySafe(200, cancellationToken);
        }
        public async Task ShowColorReplica(ICharacter? character, SystemColor backgroundColor, SystemColor textColor, IResourceString text, CancellationToken cancellationToken)
        {
            await ShowReplica(character, text, cancellationToken);
        }
        public async Task ShowFullscreenReplica(ICharacter? character, IResourceItem? background, IResourceString text, CancellationToken cancellationToken)
        {
            await ShowReplica(character, text, cancellationToken);
        }

        public async Task<int> ShowChoice(ICharacter? character, IStringCollection variants, CancellationToken cancellationToken)
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

            while (!isCompleted && !cancellationToken.IsCancellationRequested)
            {
                await Task.DelaySafe(50, cancellationToken);
            }

            block.ChoiceChanged -= OnBlockChoiceChanged;

            return result;
        }

        public async Task HandleTrigger(string name, CancellationToken cancellationToken)
        {
            AddElement(null, $"Событие: {name}");
        }

        private void AddElement(UIElement element)
        {
            _history.Children.Add(element);
        }
        private void AddElement(ICharacter? character, string text)
        {
            var block = _textPool.GetElement();
            block.TextWrapping = TextWrapping.Wrap;
            block.RemoveFromParent();
                
            var container = GetNewBlock();
            container.Preview = block;

            if (character != null)
            {
                block.Text = $"{character.Name}: {text}";
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

        #endregion

        #region События

        public void OnDialogExecutingEnded(object? sender, EventArgs e)
        {
            AddElement(null, "Диалог завершён");
        }
        public void OnDialogExecutingStarted(object? sender, EventArgs e)
        {
            AddElement(null, "Диалог начат");
        }

        #endregion
    }
}
