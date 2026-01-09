using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

        public static ReadOnlyCollection<ReadOnlyCollection<DialogProjectDialogNode>> CreateGroups(DialogProjectDialog dialog)
        {
            HashSet<HashSet<DialogProjectDialogNode>> groups = [];
            HashSet<DialogProjectDialogNode> addedInGroups = [];

            void CheckNode(DialogProjectDialogNode node, HashSet<DialogProjectDialogNode> group)
            {
                if (addedInGroups.Contains(node))
                {
                    return;
                }
                if (node is DialogProjectReplicaNode replica && 
                    replica.Input.ConnectionsCount == 2 &&
                    replica.Text.ConnectionsCount == 1 &&
                    replica.Text[0].Node is DialogProjectStringNode strNode &&
                    strNode.Value?.Contains("передумаете") == true)
                {
                    Debug.WriteLine(node);
                }

                addedInGroups.Add(node);
                List<DialogProjectDialogNode> connectedNodes = [];

                foreach (var port in node.GetOutputs().Keys)
                {
                    foreach (var connection in port)
                    {
                        if (connection.Node is DialogProjectDialogNode dialogNode)
                        {
                            connectedNodes.Add(dialogNode);
                        }
                    }
                }

                foreach (var input in node.GetInputs().Keys)
                {
                    if (input.ConnectionsCount > 1)
                    {
                        foreach (var connection in input)
                        {
                            if (connection.Node is DialogProjectDialogNode dialogNode)
                            {
                                CheckNode(dialogNode, []);
                            }
                        }

                        if (group.Count > 0)
                        {
                            group = [];
                        }
                    }
                    else if (input.ConnectionsCount == 1 && input[0].Node is DialogProjectDialogNode dialogNode)
                    {
                        CheckNode(dialogNode, group);
                    }
                }

                groups.Add(group);
                group.Add(node);

                if (connectedNodes.Count > 1)
                {
                    foreach (var connectedNode in connectedNodes)
                    {
                        CheckNode(connectedNode, []);
                    }

                    return;
                }

                if (connectedNodes.Count == 1)
                {
                    CheckNode(connectedNodes[0], group);
                }

                groups.Add(group);
            }

            foreach (var node in dialog.Nodes)
            {
                if (addedInGroups.Contains(node))
                {
                    continue;
                }

                CheckNode(node, []);
            }

            int removedGroups = groups.RemoveWhere(g => g.Count == 0);

            return new([.. groups.Select(g => new ReadOnlyCollection<DialogProjectDialogNode>([.. g]))]);
        }
        public static DialogActionsMap Create(DialogProjectDialog dialog)
        {
            List<DialogProjectDialogNode> entryNodes = [];
            List<List<DialogProjectDialogNode>> actionGroups = [[]];

            foreach (var node in dialog.Nodes)
            {
                bool canBeEntryPoint = node.CanBeEntryPoint;
                bool isImmediate = node.IsImmediate;

                if (canBeEntryPoint && isImmediate && node.IsUserHandleNode)
                {
                    entryNodes.Add(node);
                }
            }

            //return new(new(entryNodes), CreateGroups(dialog));

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
                        foreach (var connection in output)
                        {
                            if (connection.ConnectionsCount > 1)
                            {
                                manyConnections = true;
                                break;
                            }
                        }

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
