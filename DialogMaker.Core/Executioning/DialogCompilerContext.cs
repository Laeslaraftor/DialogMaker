using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;
using System;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogCompilerContext(DialogCompiler compiler, DialogSectionBuilder section, DialogCompilerResources resources, HashSet<INode> compiledNodes, Dictionary<INode, int> nodeIndexes)
    {
        private DialogCompilerContext(DialogCompilerContext other, DialogSectionBuilder section)
            : this(other.Compiler, section, other.Resources, other._compiledNodes, other._nodeIndexes)
        {
        }

        public DialogCompiler Compiler { get; } = compiler;
        public DialogSectionBuilder Section { get; } = section;
        public DialogCompilerResources Resources { get; } = resources;

        private readonly HashSet<INode> _compiledNodes = compiledNodes;
        private readonly Dictionary<INode, int> _nodeIndexes = nodeIndexes;

        #region Управление

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

            DialogByteCode code;
            bool equalsSections = section == Section;

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

                int operationIndex = GetNodeIndex(node);
                action.Arguments[0] = new(Section.Operations[operationIndex]);
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

        public bool IsCompiled(INode node) => _compiledNodes.Contains(node);
        public int GetNodeIndex(INode node)
        {
            if (_nodeIndexes.TryGetValue(node, out var index))
            {
                return index;
            }

            return -1;
        }
        public bool Compile(INode node)
        {
            if (_compiledNodes.Contains(node))
            {
                return false;
            }

            int currentIndex = Section.Operations.Count;

            _compiledNodes.Add(node);
            DialogCompilerContext context = this;

            if (node is DialogProjectDialogNode dialogNode &&
                Compiler.Sections.TryGetValue(dialogNode, out var nodeSection) &&
                nodeSection != Section)
            {
                context = new(this, nodeSection);
            }

            node.Compile(context);

            if (Section.Operations.Count != currentIndex)
            {
                _nodeIndexes.Add(node, currentIndex);
            }

            return true;
        }
        public void CompileOutputs(DialogProjectNodeOutput output)
        {
            Compiler.CompileOutputs(this, output);
        }
        public DialogExecutionParameter RecursiveCompileConnections(DialogProjectNodeInput input)
        {
            return Compiler.RecursiveCompileConnections(this, input);
        }

        #endregion
    }
}
