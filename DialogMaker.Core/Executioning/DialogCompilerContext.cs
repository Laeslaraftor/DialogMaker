using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogCompilerContext(DialogCompiler compiler, DialogSectionBuilder section, DialogCompilerResources resources, HashSet<INode> compiledNodes, Dictionary<INode, int> nodeIndexes)
    {
        public DialogCompiler Compiler { get; } = compiler;
        public DialogSectionBuilder Section { get; } = section;
        public DialogCompilerResources Resources { get; } = resources;

        private readonly HashSet<INode> _compiledNodes = compiledNodes;
        private readonly Dictionary<INode, int> _nodeIndexes = nodeIndexes;

        #region Управление

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
            node.Compile(this);

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
