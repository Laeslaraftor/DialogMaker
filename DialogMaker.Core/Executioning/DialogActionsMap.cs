using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning
{
    public class DialogActionsMap(ReadOnlyCollection<DialogProjectDialogNode> entries, ReadOnlyCollection<DialogProjectDialogNode> actions)
    {
        public ReadOnlyCollection<DialogProjectDialogNode> EntryNodes { get; } = entries;
        public ReadOnlyCollection<DialogProjectDialogNode> ActionNodes { get; } = actions;

        private readonly Dictionary<DialogProjectDialogNode, ReadOnlyCollection<DialogProjectDialogNode>> _nextNodes = [];
        private readonly Dictionary<DialogProjectDialogNode, ReadOnlyCollection<DialogProjectDialogNode>> _previousNodes = [];

        #region Управление

        public ReadOnlyCollection<DialogProjectDialogNode> GetNextNodes(DialogProjectDialogNode node)
        {
            if (_nextNodes.TryGetValue(node, out var nextNodes))
            {
                return nextNodes;
            }

            List<DialogProjectDialogNode> nodes = [];

            foreach (var output in node.GetOutputs().Keys)
            {
                foreach (var connection in output.Connections)
                {
                    if (connection.Node is DialogProjectDialogNode dialogNode)
                    {
                        nodes.Add(dialogNode);
                    }
                }
            }

            nextNodes = new(nodes);
            _nextNodes.Add(node, nextNodes);

            return nextNodes;
        }
        public ReadOnlyCollection<DialogProjectDialogNode> GetPreviousNodes(DialogProjectDialogNode node)
        {
            if (_previousNodes.TryGetValue(node, out var previousNodes))
            {
                return previousNodes;
            }

            List<DialogProjectDialogNode> nodes = [];

            foreach (var input in node.GetInputs().Keys)
            {
                foreach (var connection in input.Connections)
                {
                    if (connection.Node is DialogProjectDialogNode dialogNode)
                    {
                        nodes.Add(dialogNode);
                    }
                }
            }

            previousNodes = new(nodes);
            _previousNodes.Add(node, previousNodes);

            return previousNodes;
        }

        #endregion

        #region Статика

        public static DialogActionsMap Create(DialogProjectDialog dialog)
        {
            List<DialogProjectDialogNode> entryNodes = [];
            List<DialogProjectDialogNode> actionNodes = [];

            foreach (var node in dialog.Nodes)
            {
                int actionPortsCount = 0;

                foreach (var actionPort in node.GetPorts(p => p.DataType == DialogNodePortType.Action))
                {
                    actionPortsCount++;

                    if (actionPort is DialogProjectNodeInput &&
                        actionPort.ConnectionsCount == 0)
                    {
                        entryNodes.Add(node);
                        break;
                    }
                }

                if (actionPortsCount != 0)
                {
                    actionNodes.Add(node);
                }
            }

            return new(new(entryNodes), new(actionNodes));
        }

        #endregion
    }
}
