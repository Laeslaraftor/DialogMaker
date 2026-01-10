using DialogMaker.Core.Editor.Nodes;
using System.Collections.Generic;
using System.IO;

namespace DialogMaker.Core.Executioning.Builders
{
    public readonly struct CodeCompileContext(Dictionary<INode, DialogCompilerNodeInfo> nodesInfo, Stream codeStream, DialogExecutionContextBuilder context)
    {
        public Dictionary<INode, DialogCompilerNodeInfo> NodesInfo { get; } = nodesInfo;
        public Stream CodeStream { get; } = codeStream;
        public DialogExecutionContextBuilder ContextBuilder { get; } = context;
    }
}
