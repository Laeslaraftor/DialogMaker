using Acly;

namespace DialogMaker.Core.Editor
{
    public class ContextMenuSource
    {
        public ContextMenuSource(ObservableList<ContextMenuAction> actions)
        {
            Items = new(actions);
        }

        public ReferenceReadOnlyList<ContextMenuAction> Items { get; }
    }
}
