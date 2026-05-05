using DialogMaker.Lib.Commands;
using System.Collections.Specialized;
using System.Windows;

namespace DialogMaker.Editor.Nodes
{
    public class ProjectNodesClipboard : Disposable
    {
        public ProjectNodesClipboard(ProjectDialog dialog)
        {
            Dialog = dialog;
            CanCopy = dialog.SelectedNodes.Count > 0;

            CopyCommand = new(this, nameof(CanCopy), p => Copy(), p => CanCopy);
            CutCommand = new(this, nameof(CanCopy), p => Cut(), p => CanCopy);
            PasteCommand = new(this, nameof(CanPaste), p => Paste(), p => CanPaste);

            dialog.SelectedNodes.CollectionChanged += OnSelectedNodesCollectionChanged;
        }

        public ProjectDialog Dialog { get; }
        public bool CanCopy
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(CanCopy));
                    field = value;
                    OnPropertyChanged(nameof(CanCopy));
                }
            }
        }
        public bool CanPaste
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(CanPaste));
                    field = value;
                    OnPropertyChanged(nameof(CanPaste));
                }
            }
        }
        public PropertyCommand CopyCommand { get; }
        public PropertyCommand CutCommand { get; }
        public PropertyCommand PasteCommand { get; }

        private ProjectNodesCopiedState? _copiedState;

        #region Управление

        public bool Copy()
        {
            if (Dialog.SelectedNodes.Count == 0)
            {
                return false;
            }

            try
            {
                _copiedState?.Dispose();
                var savedStates = Dialog.SelectedNodes.Select(n => n.Original.Save()).ToList();
                _copiedState = new(savedStates);
            }
            catch (Exception error)
            {
                error.Log();
                return false;
            }

            CanPaste = true;

            return true;
        }
        public void Cut()
        {
            if (Copy())
            {
                Dialog.RemoveSelectedNodes();
            }
        }
        public void Paste(Point mousePosition)
        {
            if (!CanPaste || _copiedState == null)
            {
                return;
            }

            Point center = new();
            int count = 0;
            var nodes = _copiedState.Paste(Dialog.Original).ToList();

            foreach (var node in nodes)
            {
                center += node.Position;
                count++;
            }

            center /= count;
            center = (Point)(mousePosition - center);

            foreach (var node in nodes)
            {
                node.Position += center;
            }
        }
        public void Paste() => Paste(Dialog.LastMouseClickPosition);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            CopyCommand.Dispose();
            CutCommand.Dispose();
            PasteCommand.Dispose();

            Dialog.SelectedNodes.CollectionChanged -= OnSelectedNodesCollectionChanged;
        }

        #endregion

        #region События

        private void OnSelectedNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CanCopy = Dialog.SelectedNodes.Count > 0;
        }

        #endregion
    }
}
