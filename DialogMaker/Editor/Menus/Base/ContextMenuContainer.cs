using System.Windows.Controls;

namespace DialogMaker.Editor.Menus
{
    public class ContextMenuContainer(string name, IEnumerable<IContextMenuModifier> modifiers) : IContextMenuModifier
    {
        public string Name { get; } = name;
        public IEnumerable<IContextMenuModifier> Modifiers { get; } = modifiers;

        #region Управление

        public void Modify(ItemCollection menu)
        {
            MenuItem item = new()
            {
                Name = Name
            };

            foreach (var modifier in Modifiers)
            {
                modifier.Modify(item.Items);
            }

            menu.Add(item);
        }

        #endregion
    }
}
