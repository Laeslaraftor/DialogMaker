using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using DialogMaker.Core.Executioning.Debugging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DialogMaker.Core.Executioning
{
    public class DialogActionsMap
    {
        public DialogActionsMap(ReadOnlyCollection<DialogProjectDialogNode> entries, ReadOnlyCollection<ReadOnlyCollection<DialogProjectDialogNode>> groups)
        {
            EntryNodes = entries;
            ActionGroups = groups;
        }

        public ReadOnlyCollection<DialogProjectDialogNode> EntryNodes { get; }
        public ReadOnlyCollection<ReadOnlyCollection<DialogProjectDialogNode>> ActionGroups { get; }

        #region Статика

        public static List<DialogProjectDialogNode> GetEntries(DialogProjectDialog dialog)
        {
            List<DialogProjectDialogNode> result = [];

            foreach (var node in dialog.Nodes)
            {
                bool canBeEntryPoint = node.CanBeEntryPoint;
                bool isImmediate = node.IsImmediate;

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

            var structure = CreateStructure(dialog);

            foreach (var section in structure.Sections)
            {
                List<DialogProjectDialogNode> group = [.. section.Items.Keys.Cast<DialogProjectDialogNode>()];

                if (group.Count > 0)
                {
                    actionGroups.Add(group);
                }
            }

            return new(new(entryNodes), new([.. actionGroups.Select(c => new ReadOnlyCollection<DialogProjectDialogNode>(c))]));

            bool AlreadyInGroup(DialogProjectDialogNode node)
            {
                foreach (var group in actionGroups)
                {
                    foreach (var value in group)
                    {
                        if (value == node)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            void CheckConnections(List<DialogProjectDialogNode> group, ICollection<DialogProjectNodeOutput> outputs, bool forceNextGroups = false)
            {
                bool isEmpty = true;

                foreach (var output in outputs)
                {
                    if (forceNextGroups || output.ConnectionsCount > 1)
                    {
                        foreach (var connection in output)
                        {
                            isEmpty = false;

                            if (connection.Node is not DialogProjectDialogNode dialogNode)
                            {
                                continue;
                            }

                            List<DialogProjectDialogNode> nextGroup = [];
                            actionGroups.Add(nextGroup);
                            CheckGroups(nextGroup, dialogNode);
                        }

                        continue;
                    }

                    foreach (var connection in output)
                    {
                        isEmpty = false;

                        if (connection.Node is DialogProjectDialogNode dialogNode)
                        {
                            CheckGroups(group, dialogNode);
                        }
                    }
                }

                if (outputs.Count > 0 && isEmpty)
                {
                    actionGroups.Add([]);
                }
            }
            void CheckGroups(List<DialogProjectDialogNode> group, DialogProjectDialogNode node)
            {
                if (AlreadyInGroup(node))
                {
                    return;
                }

                HashSet<INode> uniqueInputNodes = [];

                foreach (var input in node.GetInputs().Keys)
                {
                    uniqueInputNodes.Clear();

                    foreach (var connection in input)
                    {
                        uniqueInputNodes.Add(connection.Node);
                    }

                    if (uniqueInputNodes.Count > 1)
                    {
                        group = [];
                        actionGroups.Add(group);
                        break;
                    }
                }

                group.Add(node);

                var outputs = node.GetOutputs().Keys;

                if (outputs.Count == 1)
                {
                    CheckConnections(group, outputs);
                }
                else if (outputs.Count > 1)
                {
                    bool manyConnections = false;
                    int lastConnectionCount = 0;

                    foreach (var output in outputs)
                    {
                        if (output.ConnectionsCount > 0 && lastConnectionCount > 0)
                        {
                            manyConnections = true;
                            break;
                        }
                        //foreach (var connection in output)
                        //{
                        //    if (connection.ConnectionsCount > 1)
                        //    {
                        //        manyConnections = true;
                        //        break;
                        //    }
                        //}

                        lastConnectionCount = Math.Max(output.ConnectionsCount, lastConnectionCount);
                    }

                    if (manyConnections)
                    {
                        //List<DialogProjectDialogNode> newGroup = [];
                        //actionGroups.Add(newGroup);
                        CheckConnections(group, outputs, true);
                    }
                    else
                    {
                        CheckConnections(group, outputs);
                    }
                }
            }

            foreach (var node in dialog.Nodes)
            {
                CheckGroups(actionGroups[^1], node);
            }

            actionGroups.RemoveAll(g => g.Count == 0);

            return new(new(entryNodes), new([.. actionGroups.Select(c => new ReadOnlyCollection<DialogProjectDialogNode>(c))]));
        }

        #endregion
    }
}
