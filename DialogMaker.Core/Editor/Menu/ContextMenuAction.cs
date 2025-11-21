using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;

namespace DialogMaker.Core.Editor
{
    public class ContextMenuAction
    {
        public ContextMenuAction(string name, ICommand command)
            : this(name, command, null, null)
        {
        }
        public ContextMenuAction(string name, ICommand command, Color color)
            : this(name, command, color, null)
        {
        }
        public ContextMenuAction(string name, ICommand command, Color? color, IList<ContextMenuAction>? items)
        {
            Name = name;
            Color = color;
            Command = command;

            items ??= new List<ContextMenuAction>();

            Items = new(items);
        }

        public string Name { get; }
        public Color? Color { get; } 
        public ICommand Command { get; }
        public ReadOnlyCollection<ContextMenuAction> Items { get; }
    }
}
