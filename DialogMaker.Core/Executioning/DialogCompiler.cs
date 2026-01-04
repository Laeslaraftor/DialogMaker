using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;
using System.Linq;

namespace DialogMaker.Core.Executioning
{
    public class DialogCompiler
    {
        public DialogCompiler(DialogActionsMap map)
        {
            CodeBuilder = new();
            Map = map;

            foreach (var action in map.ActionNodes)
            {
                _sections.Add(action, CodeBuilder.CreateSection());
            }

            Sections = new(_sections);
        }

        public DialogCodeBuilder CodeBuilder { get; }
        public DialogActionsMap Map { get; }
        public ReferenceReadOnlyDictionary<DialogProjectDialogNode, DialogSectionBuilder> Sections { get; }

        private readonly ObservableDictionary<DialogProjectDialogNode, DialogSectionBuilder> _sections = [];

        #region Управление

        public List<DialogSectionBuilder> GetConnectedSections(DialogProjectNodeOutputAction output)
        {
            List<DialogSectionBuilder> result = [];

            if (output.ConnectionsCount > 0)
            {
                foreach (var connection in output.Connections)
                {
                    if (connection.Node is DialogProjectDialogNode node &&
                        _sections.TryGetValue(node, out var section))
                    {
                        result.Add(section);
                    }
                }
            }

            return result;
        }

        public DialogExecutionParameter RecursiveCompileConnections(DialogCompilerContext context, DialogProjectNodeInput input)
        {
            if (input.ConnectionType != DialogNodeConnectionType.Action)
            {
                foreach (var connection in input)
                {
                    context.Compile(connection.Node);
                    return context.Resources.GetOrCreateVariable(connection);
                }
            }

            return context.Resources.GetOrCreateVariable(input);
        }
        public void CompileOutputs(DialogCompilerContext context, DialogProjectNodeOutput output)
        {
            if (output.ConnectionsCount == 0)
            {
                return;
            }
            if (output.ConnectionsCount == 1)
            {
                if (output.FirstOrDefault()?.Node is not DialogProjectDialogNode nextNode ||
                    !_sections.TryGetValue(nextNode, out var section))
                {
                    return;
                }

                var jump = context.Section.CreateOperation(DialogByteCode.Jump);
                jump.Arguments[0] = new(section);

                return;
            }

            foreach (var connection in output)
            {
                if (connection.Node is not DialogProjectDialogNode node ||
                    !_sections.TryGetValue(node, out var section))
                {
                    continue;
                }

                var threadStart = context.Section.CreateOperation(DialogByteCode.StartThread);
                threadStart.Arguments[0] = new(section);
            }
        }

        public CompiledCodeInfo Compile()
        {
            foreach (var section in _sections.Values)
            {
                section.Clear();
            }

            DialogCompilerResources resources = new();
            HashSet<INode> compiledNodes = [];

            foreach (var info in _sections)
            {
                DialogCompilerContext context = new(this, info.Value, resources, compiledNodes);
                context.Compile(info.Key);
            }

            return CodeBuilder.Compile();
        }

        #endregion

        #region Статика

        public static DialogCompiler Create(DialogProjectDialog dialog)
        {
            DialogActionsMap map = DialogActionsMap.Create(dialog);
            return new(map);
        }

        #endregion
    }
}
