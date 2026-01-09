using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
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
            yield return new ContextMenuContainer(Icons.Add, "Добавить", GetNodes());
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

        private IEnumerable<IContextMenuModifier> GetNodes()
        {
            Dictionary<string, List<NodeInfo>> folders = [];

            foreach (var nodeInfo in DialogProjectDialogNode.AvailableNodes)
            {
                var name = nodeInfo.Key.GetEnumAttribute<NameAttribute>()?.Name;
                var path = nodeInfo.Key.GetEnumAttribute<PathAttribute>()?.Path ?? string.Empty;
                name ??= nodeInfo.Key.ToString();

                if (!folders.TryGetValue(path, out var nodes))
                {
                    nodes = [];
                    folders.Add(path, nodes);
                }

                nodes.Add(new(name, nodeInfo.Key, nodeInfo.Value));
            }

            foreach (var info in folders)
            {
                info.Value.Sort();

                foreach (var modifier in CreateFolder(info))
                {
                    yield return modifier;
                }
            }
        }
        IEnumerable<IContextMenuModifier> CreateFolder(KeyValuePair<string, List<NodeInfo>> info)
        {
            if (info.Value.Count == 0)
            {
                return [];
            }

            var parts = info.Key.Split('/');
            var nodes = CreateNodes(info.Value);

            return CreateContainer(nodes, parts);
        }
        IEnumerable<IContextMenuModifier> CreateContainer(IEnumerable<IContextMenuModifier> nodes, string[] parts)
        {
            if (parts.Length == 0)
            {
                foreach (var node in nodes)
                {
                    yield return node;
                }
            }

            ContextMenuContainer Create(int index)
            {
                IEnumerable<IContextMenuModifier> next = nodes;

                if (index + 1 < parts.Length)
                {
                    next = [Create(index + 1)];
                }

                return new ContextMenuContainer(parts[index], next);
            }

            yield return Create(0);
        }
        private IEnumerable<IContextMenuModifier> CreateNodes(IEnumerable<NodeInfo> nodesInfo)
        {
            foreach (var info in nodesInfo)
            {
                yield return new ContextMenuAction(info.Name, p => AddNode(p, info.NodeType));
            }
        }

        #region Команды

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
                    error.Alert();
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

        #region Классы

        private readonly struct NodeInfo(string name, DialogNodeType nodeType, Type type) : IComparable
        {
            public string Name { get; } = name;
            public DialogNodeType NodeType { get; } = nodeType;
            public Type Type { get; } = type;

            public int CompareTo(object? obj)
            {
                if (obj is NodeInfo other)
                {
                    return Name.CompareTo(other.Name);
                }

                return -1;
            }
        }

        #endregion
    }
}
