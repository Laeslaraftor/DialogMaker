using DialogMaker.Core.Editor.Nodes;
using System.Collections;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning.Debugging
{
    public readonly struct DialogCodeStructureItem : IEnumerable<INode>
    {
        public DialogCodeStructureItem(INode node, IList<DialogCodeStructureItem>? references = null, IList<INode>? dataNodes = null)
        {
            Node = node;

            if (references != null)
            {
                References = new(references);
            }
            if (dataNodes != null)
            {
                DataNodes = new(dataNodes);
            }
        }

        public INode Node { get; }
        public ReadOnlyCollection<DialogCodeStructureItem>? References { get; }
        public ReadOnlyCollection<INode>? DataNodes { get; }

        #region Перечисление

        public IEnumerator<INode> GetEnumerator()
        {
            yield return Node;

            if (DataNodes != null)
            {
                foreach (var node in DataNodes)
                {
                    yield return node;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Статика

        public static DialogCodeStructureItem Create(INode node)
        {
            return Create(node, i => true);
        }
        public static DialogCodeStructureItem Create(INode node, Predicate<DialogProjectNodePort> inputPredicate)
        {
            List<INode>? data = null;
            List<DialogCodeStructureItem>? references = null;
            bool isFunction = node.IsFunction;

            foreach (var input in node.GetInputs().Keys)
            {
                if (input.ConnectionsCount == 0)
                {
                    continue;
                }
                foreach (var connection in input)
                {
                    if (!inputPredicate(connection) ||
                        isFunction && connection.Node.IsCodeGenerator)
                    {
                        continue;
                    }
                    if (connection.Node.IsCodeGenerator)
                    {
                        references ??= [];
                        var referencedItem = Create(connection.Node, inputPredicate);
                        references.Add(referencedItem);

                        continue;
                    }

                    data ??= [];
                    data.Add(connection.Node);
                }
            }

            references?.Invert();

            return new(node, references, data);
        }

        #endregion
    }
}
