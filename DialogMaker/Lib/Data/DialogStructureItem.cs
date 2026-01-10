using DialogMaker.Core.Executioning.Debugging;
using DialogMaker.Editor;
using System.Collections.ObjectModel;

namespace DialogMaker.Lib.Data
{
    public class DialogStructureItem(DialogStructureSection section, DialogCodeStructureItem item)
    {
        public DialogStructureSection Section { get; } = section;
        public DialogCodeStructureItem Item { get; } = item;
        public DialogProjectNode? Node
        {
            get
            {
                if (field == null && !_nodeIsFound)
                {
                    field = Section.Dialog[Item.Node];
                    _nodeIsFound = true;
                }

                return field;
            }
        }
        public ReadOnlyCollection<DialogStructureItem>? References
        {
            get
            {
                if (Item.References == null)
                {
                    return null;
                }
                if (field == null)
                {
                    List<DialogStructureItem> items = [];

                    foreach (var reference in Item.References)
                    {
                        items.Add(new(Section, reference));
                    }

                    field = new(items);
                }

                return field;
            }
        }
        public ReadOnlyCollection<DialogProjectNode>? DataNodes
        {
            get
            {
                if (Item.DataNodes == null)
                {
                    return null;
                }
                if (field == null)
                {
                    List<DialogProjectNode> nodes = [];

                    foreach (var node in Item.DataNodes)
                    {
                        nodes.Add(Section.Dialog[node]);
                    }

                    field = new(nodes);
                }

                return field;
            }
        }

        private bool _nodeIsFound;
    }
}
