using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogCompilerContext(DialogCompiler compiler, DialogSectionBuilder section, DialogCompilerResources resources, HashSet<INode> compiledNodes, Dictionary<INode, DialogCompilerNodeInfo> nodeIndexes)
    {
        private DialogCompilerContext(DialogCompilerContext other, DialogSectionBuilder section)
            : this(other.Compiler, section, other.Resources, other._compiledNodes, other._nodesInfo)
        {
        }

        public DialogCompiler Compiler { get; } = compiler;
        public DialogSectionBuilder Section { get; } = section;
        public DialogCompilerResources Resources { get; } = resources;

        private readonly HashSet<INode> _compiledNodes = compiledNodes;
        private readonly Dictionary<INode, DialogCompilerNodeInfo> _nodesInfo = nodeIndexes;

        #region Построение

        public OperationBuilder JumpOrGoto(INode node, bool conditional = false)
        {
            if (node is DialogProjectDialogNode dialogNode)
            {
                return JumpOrGoto(dialogNode, conditional);
            }

            throw new InvalidCastException();
        }
        public OperationBuilder JumpOrGoto(DialogProjectDialogNode node, bool conditional = false)
        {
            if (!Compiler.Sections.TryGetValue(node, out var section))
            {
                throw new ArgumentException($"Не удалось получить сегмент для узла: {node}", nameof(node));
            }

            var realSection = Compiler.ToRealSection(Section);
            DialogByteCode code;
            bool equalsSections = section == realSection;

            if (conditional)
            {
                code = equalsSections ? DialogByteCode.GotoIfTrue : DialogByteCode.JumpIfTrue;
            }
            else
            {
                code = equalsSections ? DialogByteCode.Goto : DialogByteCode.Jump;
            }

            var action = Section.CreateOperation(code);

            if (equalsSections)
            {
                if (!IsCompiled(node))
                {
                    Compile(node);
                }

                //int operationIndex = GetNodeIndex(node);
                //action.Arguments[0] = new(Section.Operations[operationIndex]);
                action.Arguments[0] = new(node);
                return action;
            }

            action.Arguments[0] = new(section);

            return action;
        }
        public OperationBuilder JumpOrGotoOrSkip(INode node, DialogExecutionParameter valueVariable, bool skipValue)
        {
            if (node is DialogProjectDialogNode dialogNode)
            {
                return JumpOrGotoOrSkip(dialogNode, valueVariable, skipValue);
            }

            throw new InvalidCastException();
        }
        public OperationBuilder JumpOrGotoOrSkip(DialogProjectDialogNode node, DialogExecutionParameter valueVariable, bool skipValue)
        {
            var skipComparison = Section.CreateOperation(DialogByteCode.Equals);
            skipComparison.Arguments[0] = valueVariable;
            skipComparison.Arguments[1] = new(skipValue);

            var skipOpCode = Section.CreateOperation(DialogByteCode.GotoIfTrue);

            JumpOrGoto(node);

            var empty = Section.CreateOperation(DialogByteCode.Empty);
            skipOpCode.Arguments[0] = new(empty);

            return skipComparison;
        }

        public void CompileOutputs(DialogProjectNodeOutput output, bool onlyStartThreads = false, bool forceToCurrent = false)
        {
            bool noOutputsAtAll = true;
            bool dontJump = false;

            if (output.Node is DialogProjectDialogNode dialogNode)
            {
                foreach (var port in dialogNode.GetOutputs().Keys)
                {
                    if (port.ConnectionsCount > 0)
                    {
                        noOutputsAtAll = false;
                        break;
                    }
                }
            }

            if (noOutputsAtAll)
            {
                Section.CreateOperation(DialogByteCode.EndThread);
                return;
            }
            if (output.ConnectionsCount == 0)
            {
                return;
            }
            if (!Compiler.Sections.TryGetValue((DialogProjectDialogNode)output.Node, out var realSection))
            {
                if (!output.Node.IsImmediate ||
                    output.ConnectionsCount != 1 ||
                    !Compiler.Sections.TryGetValue((DialogProjectDialogNode)output.Connections[0].Node, out realSection))
                {
                    throw new ArgumentException($"Не удалось получить сегмент для узла {output.Node}", nameof(output));
                }
                else
                {
                    dontJump = true;
                }
            }
            if (output.ConnectionsCount == 1 && !onlyStartThreads)
            {
                if (output.FirstOrDefault()?.Node is not DialogProjectDialogNode nextNode ||
                    !Compiler.Sections.TryGetValue(nextNode, out var section))
                {
                    return;
                }

                bool sectionsEquals = section == realSection;
                bool isCompiled = IsCompiled(nextNode);

                if (sectionsEquals && !isCompiled)
                {
                    Compile(nextNode, forceToCurrent);
                    return;
                }
                if (!dontJump)
                {
                    DialogByteCode code;

                    if (onlyStartThreads)
                    {
                        code = DialogByteCode.StartThread;
                    }
                    else
                    {
                        code = sectionsEquals ? DialogByteCode.Goto : DialogByteCode.Jump;
                    }

                    var opCode = Section.CreateOperation(code);

                    if (sectionsEquals)
                    {
                        opCode.Arguments[0] = new(nextNode);
                        return;
                    }

                    opCode.Arguments[0] = new(section);
                }

                Compile(nextNode);

                return;
            }

            int count = 0;
            Dictionary<DialogProjectDialogNode, OperationBuilder> jumpOperations = [];

            foreach (var connection in output)
            {
                if (connection.Node is not DialogProjectDialogNode node ||
                    !Compiler.Sections.TryGetValue(node, out var section))
                {
                    count++;
                    continue;
                }

                var opCode = count + 1 >= output.ConnectionsCount && !onlyStartThreads ? DialogByteCode.JumpTo : DialogByteCode.StartThread2;
                var threadStart = Section.CreateOperation(opCode);
                threadStart.Arguments[0] = new(section);
                jumpOperations.Add(node, threadStart);

                count++;
            }
            foreach (var info in jumpOperations)
            {
                Compile(info.Key);
                info.Value.Arguments[1] = new(info.Key);
            }
        }

        #endregion

        #region Управление

        public bool IsCompiled(INode node) => _compiledNodes.Contains(node);
        public int GetNodeIndex(INode node)
        {
            if (_nodesInfo.TryGetValue(node, out var info))
            {
                return info.Index;
            }

            return -1;
        }
        public OperationBuilder GetNodeOperation(INode node)
        {
            if (_nodesInfo.TryGetValue(node, out var info))
            {
                return info.FirstOperation;
            }

            throw new ArgumentException($"Оператор для узла {node} не найден!", nameof(node));
        }
        public bool TryGetNodeInfo(INode node, [NotNullWhen(true)] out DialogCompilerNodeInfo result)
        {
            return _nodesInfo.TryGetValue(node, out result);
        }
        public bool Compile(INode node, bool forceToCurrent = false)
        {
            if (_compiledNodes.Contains(node))
            {
                return false;
            }


            _compiledNodes.Add(node);
            DialogCompilerContext context = this;

            if (!forceToCurrent && node is DialogProjectDialogNode dialogNode &&
                Compiler.IndividualSections.TryGetValue(dialogNode, out var nodeSection) &&
                nodeSection != Section)
            {
                context = new(this, nodeSection);
            }

            int currentIndex = context.Section.Operations.Count;
            _nodesInfo.Add(node, new(node, currentIndex, context.Section, 0));

            node.Compile(context);

            if (context.Section.Operations.Count > currentIndex)
            {
                var size = context.Section.Operations.Count - currentIndex;
                _nodesInfo[node] = new(node, currentIndex, context.Section, size);
            }
            else
            {
                _nodesInfo.Remove(node);
            }

            return true;
        }
        public DialogExecutionParameter RecursiveCompileConnections(DialogProjectNodeInput input)
        {
            return Compiler.RecursiveCompileConnections(this, input);
        }

        #endregion
    }
    public readonly struct DialogCompilerNodeInfo(INode node, int index, DialogSectionBuilder section, int size)
    {
        public INode Node { get; } = node;
        public int Index { get; } = index;
        public OperationBuilder FirstOperation => Section.Operations[Index];
        public DialogSectionBuilder Section { get; } = section;
        public int Size { get; } = size;
    }
}
