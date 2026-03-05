using DialogMaker.Core;
using DialogMaker.Lib;
using DialogMaker.Lib.Controllers;
using System.Windows.Input;

namespace DialogMaker.Editor.Menus
{
    public class DialogEditorContextMenu : TypeContextMenu<ProjectDialog>
    {
        public DialogEditorContextMenu()
        {
        }
        public DialogEditorContextMenu(ProjectDialog item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Добавить", AddNode, Icons.Add)
            {
                Shortcut = $"Shift+A"
            };
            yield return ContextMenuSeparator.Instance;

            foreach (var modifier in CreateClipboardModifiers(Item))
            {
                yield return modifier;
            }

            yield return new ContextMenuAction("Удалить",
                CanDelete, RemoveDialog, Icons.Delete)
            {
                Shortcut = Key.Delete.ToString()
            };
        }

        #region Команды

        private void AddNode(object? parameter)
        {
            Resolve(parameter, async dialog =>
            {
                await NodeSelectorController.Request(dialog, null, dialog.LastMouseClickPosition);
            });
        }
        private void AddNode(object? parameter, DialogNodeType nodeType)
        {
            Resolve(parameter, dialog =>
            {
                try
                {
                    var node = dialog.Original.CreateNode(nodeType);
                    var position = dialog.LastMouseClickPosition;
                    node.Position = new((float)position.X, (float)position.Y);
                }
                catch (Exception error)
                {
                    error.Log();
                }
            });
        }

        private bool CanDelete(object? parameter)
        {
            return Resolve(parameter, dialog =>
            {
                return dialog.SelectedNodes.Count > 0;
            });
        }

        private void RemoveDialog(object? parameter)
        {
            Resolve(parameter, dialog =>
            {
                dialog.RemoveSelectedNodes();
            });
        }

        #endregion

        #region Статика

        public static readonly DialogContextMenu Instance = new();

        public static IEnumerable<IContextMenuModifier> CreateClipboardModifiers(ProjectDialog? dialog)
        {
            if (dialog == null)
            {
                yield break;
            }

            yield return new ContextMenuAction("Копировать",
                    dialog.Clipboard.CopyCommand, Icons.Copy)
            {
                Shortcut = "Ctrl+C"
            };
            yield return new ContextMenuAction("Вырезать",
                    dialog.Clipboard.CutCommand, Icons.Cut)
            {
                Shortcut = "Ctrl+X"
            };
            yield return new ContextMenuAction("Вставить",
                    dialog.Clipboard.PasteCommand, Icons.Paste)
            {
                Shortcut = "Ctrl+V"
            };
            yield return ContextMenuSeparator.Instance;
        }

        #endregion
    }
}
