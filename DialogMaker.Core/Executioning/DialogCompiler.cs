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

            if (map.EntryNodes.Count > 1)
            {
                EntrySection = CodeBuilder.CreateSection();
            }

            //foreach (var action in map.ActionNodes)
            //{
            //    _sections.Add(action, CodeBuilder.CreateSection());
            //}
            foreach (var group in map.ActionGroups)
            {
                var section = CodeBuilder.CreateSection();

                foreach (var node in group)
                {
                    _sections.Add(node, section);
                }
            }

            Sections = new(_sections);
        }

        public DialogCodeBuilder CodeBuilder { get; }
        public DialogActionsMap Map { get; }
        public ReferenceReadOnlyDictionary<DialogProjectDialogNode, DialogSectionBuilder> Sections { get; }
        public DialogSectionBuilder? EntrySection { get; }

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

                if (section == context.Section)
                {
                    if (context.IsCompiled(nextNode))
                    {
                        var index = context.GetNodeIndex(nextNode);

                        if (index == -1 || context.GetNodeIndex(output.Node) + 1 == index)
                        {
                            return;
                        }

                        var gotoOpCode = context.Section.CreateOperation(DialogByteCode.Goto);
                        gotoOpCode.Arguments[0] = new(section.Operations[index]);

                        return;
                    }

                    context.Compile(nextNode);
                    return;
                }

                var jump = context.Section.CreateOperation(DialogByteCode.Jump);
                jump.Arguments[0] = new(section);

                return;
            }

            int count = 0;

            foreach (var connection in output)
            {
                if (connection.Node is not DialogProjectDialogNode node ||
                    !_sections.TryGetValue(node, out var section))
                {
                    count++;
                    continue;
                }

                var opCode = count + 1 >= output.ConnectionsCount ? DialogByteCode.Jump : DialogByteCode.StartThread;
                var threadStart = context.Section.CreateOperation(opCode);
                threadStart.Arguments[0] = new(section);
                count++;
            }
        }

        public CompiledCodeInfo Compile()
        {
            EntrySection?.Clear();

            foreach (var section in _sections.Values)
            {
                section.Clear();
            }

            DialogCompilerResources resources = new();
            HashSet<INode> compiledNodes = [];
            Dictionary<INode, int> nodeIndexes = [];

            if (EntrySection != null)
            {
                int count = 0;

                foreach (var entry in Map.EntryNodes)
                {
                    if (!_sections.TryGetValue(entry, out var entrySection))
                    {
                        continue;
                    }

                    var opCode = count + 1 >= Map.EntryNodes.Count ? DialogByteCode.Jump : DialogByteCode.StartThread;
                    var startThread = EntrySection.CreateOperation(opCode);
                    startThread.Arguments[0] = new(entrySection);
                    count++;
                }
            }

            foreach (var info in _sections)
            {
                DialogCompilerContext context = new(this, info.Value, resources, compiledNodes, nodeIndexes);
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
