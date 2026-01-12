using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning.Builders
{
    public class JoinOperationInfoBuilder(DialogCompiler compiler, IList<INode> inputNodes, IList<INode> outputNodes)
    {
        public DialogCompiler Compiler { get; } = compiler;
        public ReadOnlyCollection<INode> InputNodes { get; } = new(inputNodes);
        public ReadOnlyCollection<INode> OutputNodes { get; } = new(outputNodes);

        #region Управление

        public IJoinOperationInfo Compile(CodeCompileContext context)
        {
            List<int> inputs = [];
            List<DialogPosition> outputs = [];
            var nodesPositions = context.NodesInfo;

            foreach (var input in InputNodes)
            {
                inputs.Add(Compiler[input].Index);
            }
            foreach (var output in OutputNodes)
            {
                if (nodesPositions.TryGetValue(output, out var info))
                {
                    outputs.Add(new(Compiler[output].Index, info.Index));
                    continue;
                }

                throw new ArgumentException($"Не удалось получить информацию о коде для узла {output}", nameof(context));
            }

            return new JoinOperationInfo(inputs, outputs);
        }

        #endregion
    }
}
