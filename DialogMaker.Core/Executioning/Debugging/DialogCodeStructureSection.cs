using DialogMaker.Core.Editor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DialogMaker.Core.Executioning.Debugging
{
    public class DialogCodeStructureSection(INode entry, IDictionary<INode, DialogCodeStructureItem> items, IList<INode> externalConnections)
    {
        public INode Entry { get; } = entry;
        public ReadOnlyDictionary<INode, DialogCodeStructureItem> Items { get; } = new(items);
        public ReadOnlyCollection<INode> ExternalConnection { get; } = new(externalConnections);

        #region Статика

        public static DialogCodeStructureSection Create(INode entry)
        {
            Dictionary<INode, DialogCodeStructureItem> items = [];
            List<INode> externalConnections = [];

            void CheckNode(INode node, INode? fromNode = null)
            {
                if (items.ContainsKey(node))
                {
                    return;
                }
                if (node.IsFunction && node != entry)
                {
                    externalConnections.Add(node);
                    return;
                }

                var item = DialogCodeStructureItem.Create(node, p =>
                {
                    return p.Node != fromNode && 
                           !p.Node.IsSeparator && 
                           !p.Node.IsFunction && 
                           p.ConnectionsCount < 2;
                });

                void AddItem(DialogCodeStructureItem item)
                {
                    if (items.ContainsKey(item.Node))
                    {
                        return;
                    }

                    if (item.References != null)
                    {
                        foreach (var reference in item.References)
                        {
                            AddItem(reference);
                        }
                    }

                    items.Add(item.Node, item);
                }

                AddItem(item);

                if (node.IsSeparator)
                {
                    return;
                }

                foreach (var output in node.GetOutputs().Keys)
                {
                    foreach (var connection in output)
                    {
                        CheckNode(connection.Node, node);
                    }
                }
            }

            CheckNode(entry);

            return new(entry, items, externalConnections);
        }

        #endregion
    }
}
