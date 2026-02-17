using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Debugging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DialogMaker.Core.Executioning
{
    public class DialogActionsMap(DialogProjectDialog dialog, IList<DialogProjectDialogNode> entries, IDictionary<DialogProjectDialogNode, DialogExecutionEvent> specialNodes, IList<ReadOnlyCollection<DialogProjectDialogNode>> groups, Dictionary<DialogProjectDialogNode, IList<DialogProjectDialogNode>> specialNodesGroup)
    {
        public DialogProjectDialog Dialog { get; } = dialog;
        public ReadOnlyCollection<DialogProjectDialogNode> EntryNodes { get; } = new(entries);
        public ReadOnlyDictionary<DialogProjectDialogNode, DialogExecutionEvent> SpecialNodes { get; } = new(specialNodes);
        public ReadOnlyDictionary<DialogProjectDialogNode, ReadOnlyCollection<DialogProjectDialogNode>> SpecialNodesGroup { get; } = specialNodesGroup.ToReadonly();
        public ReadOnlyCollection<ReadOnlyCollection<DialogProjectDialogNode>> ActionGroups { get; } = new(groups);

        #region Статика

        public static List<DialogProjectDialogNode> GetEntries(DialogProjectDialog dialog)
        {
            List<DialogProjectDialogNode> result = [];

            foreach (var node in dialog.Nodes)
            {
                bool canBeEntryPoint = node.CanBeEntryPoint;
                bool isImmediate = node.IsImmediate;

                bool CheckInputs(INode nodeToCheck, bool isFirstIteration)
                {
                    if (node.Equals(nodeToCheck) && !isFirstIteration)
                    {
                        return false;
                    }

                    foreach (var input in nodeToCheck.GetInputs().Keys)
                    {
                        foreach (var connection in input)
                        {
                            if (connection.Node.IsFunction)
                            {
                                return false;
                            }

                            return CheckInputs(connection.Node, false);
                        }
                    }

                    return true;
                }

                canBeEntryPoint = canBeEntryPoint && CheckInputs(node, true);

                if (canBeEntryPoint && isImmediate && node.IsUserHandleNode)
                {
                    result.Add(node);
                }
            }

            return result;
        }
        public static DialogCodeStructure CreateStructure(DialogProjectDialog dialog)
        {
            List<DialogProjectDialogNode> entryNodes = GetEntries(dialog);
            HashSet<INode> sectionsEntry = [];

            foreach (var entry in entryNodes)
            {
                sectionsEntry.Add(entry);
            }
            foreach (var node in dialog.Nodes)
            {
                if (node.IsFunction)
                {
                    sectionsEntry.Add(node);
                }
            }

            return DialogCodeStructure.Create([.. sectionsEntry]);
        }
        public static DialogActionsMap Create(DialogProjectDialog dialog)
        {
            List<DialogProjectDialogNode> entryNodes = GetEntries(dialog);
            List<List<DialogProjectDialogNode>> actionGroups = [];
            Dictionary<DialogProjectDialogNode, DialogExecutionEvent> specialNodes = [];
            Dictionary<DialogProjectDialogNode, IList<DialogProjectDialogNode>> specialNodesGroups = [];

            foreach (var node in dialog.Nodes)
            {
                if (!node.IsSystem)
                {
                    continue;
                }
                if (node is DialogProjectEventNode eventNode)
                {
                    specialNodes.Add(node, eventNode.Event);
                }
            }

            var structure = CreateStructure(dialog);

            foreach (var section in structure.Sections)
            {
                List<DialogProjectDialogNode> group = [.. section.Items.Keys.Cast<DialogProjectDialogNode>()];

                if (group.Count > 0)
                {
                    actionGroups.Add(group);
                }
            }

            actionGroups.RemoveAll(group =>
            {
                if (group.Count > 0 && specialNodes.ContainsKey(group[0]))
                {
                    specialNodesGroups.Add(group[0], group);
                    return true;
                }

                return false;
            });

            return new(dialog, entryNodes, specialNodes, [.. actionGroups.Select(c => new ReadOnlyCollection<DialogProjectDialogNode>(c))], specialNodesGroups);
        }

        #endregion
    }
}
