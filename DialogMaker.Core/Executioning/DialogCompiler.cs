using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;
using System;

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

            foreach (var group in map.ActionGroups)
            {
                var section = CodeBuilder.CreateSection();

                foreach (var node in group)
                {
                    _sections.Add(node, section);
                    _individualSections.Add(node, new());
                }
            }

            Sections = new(_sections);
            IndividualSections = new(_individualSections);
        }

        public DialogCodeBuilder CodeBuilder { get; }
        public DialogActionsMap Map { get; }
        public ReferenceReadOnlyDictionary<DialogProjectDialogNode, DialogSectionBuilder> Sections { get; }
        public ReferenceReadOnlyDictionary<DialogProjectDialogNode, DialogSectionBuilder> IndividualSections { get; }
        public DialogSectionBuilder? EntrySection { get; }

        private readonly ObservableDictionary<DialogProjectDialogNode, DialogSectionBuilder> _sections = [];
        private readonly ObservableDictionary<DialogProjectDialogNode, DialogSectionBuilder> _individualSections = [];

        #region Управление

        public DialogSectionBuilder ToRealSection(DialogSectionBuilder individualSection)
        {
            foreach (var info in _individualSections)
            {
                if (info.Value == individualSection)
                {
                    return _sections[info.Key];
                }
            }

            throw new ArgumentException($"Неизвестный индивидуальный сегмент: {individualSection}");
        }

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

        public CompiledCodeInfo Compile()
        {
            EntrySection?.Clear();

            foreach (var section in _sections.Values)
            {
                section.Clear();
            }
            foreach (var section in _individualSections.Values)
            {
                section.Clear();
            }

            DialogCompilerResources resources = new();
            HashSet<INode> compiledNodes = [];
            Dictionary<INode, DialogCompilerNodeInfo> nodesInfo = [];
            Dictionary<INode, DialogCompilerNodeInfo> realNodesPositions = [];

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

            foreach (var info in _individualSections)
            {
                DialogCompilerContext context = new(this, info.Value, resources, compiledNodes, nodesInfo);
                context.Compile(info.Key);
            }
            foreach (var group in Map.ActionGroups)
            {
                DialogSectionBuilder? section = null;

                foreach (var node in group)
                {
                    section ??= _sections[node];
                    var individualSection = _individualSections[node];
                    int lastPosition = section.Operations.Count;

                    if (individualSection.Operations.Count > 0)
                    {
                        individualSection.CopyTo(section);

                        int currentPosition = section.Operations.Count;

                        if (currentPosition > lastPosition)
                        {
                            realNodesPositions.Add(node, new(node, lastPosition, section, currentPosition - lastPosition));
                        }
                    }
                }
            }
            //foreach (var info in _sections)
            //{
            //    DialogCompilerContext context = new(this, info.Value, resources, compiledNodes, nodesInfo);
            //    context.Compile(info.Key);
            //}

            return CodeBuilder.Compile(realNodesPositions);
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
