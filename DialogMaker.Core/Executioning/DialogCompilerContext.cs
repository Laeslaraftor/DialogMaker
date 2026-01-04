using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogCompilerContext(DialogCompiler compiler, DialogSectionBuilder section, DialogCompilerResources resources, HashSet<INode> compiledNodes)
    {
        public DialogCompiler Compiler { get; } = compiler;
        public DialogSectionBuilder Section { get; } = section;
        public DialogCompilerResources Resources { get; } = resources;

        private readonly HashSet<INode> _compiledNodes = compiledNodes;

        #region Управление

        public bool Compile(INode node)
        {
            if (_compiledNodes.Contains(node))
            {
                return false;
            }

            _compiledNodes.Add(node);
            node.Compile(this);

            return true;
        }

        #endregion
    }
}
