using DialogMaker.Lib;
using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuContainer(string name, IEnumerable<IContextMenuModifier> modifiers) : IContextMenuModifier
    {
        public ContextMenuContainer(string icon, string name, IEnumerable<IContextMenuModifier> modifiers)
            : this(name, modifiers)
        {
            Icon = icon;
        }

        public string? Icon { get; }
        public string Name { get; } = name;
        public IEnumerable<IContextMenuModifier> Modifiers { get; } = modifiers;

        #region Управление

        public void Modify(ItemCollection menu)
        {
            MenuItem item = new()
            {
                Header = Name
            };

            if (Icons.TryCreateIconBlock(Icon, out var iconBlock))
            {
                iconBlock.FontSize = 14;
            }

            item.Icon = iconBlock;

            foreach (var modifier in Modifiers)
            {
                modifier.Modify(item.Items);
            }

            menu.Add(item);
        }

        #endregion
    }
}
