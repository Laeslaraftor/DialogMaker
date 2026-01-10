using DialogMaker.Core.Editor.Nodes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DialogMaker.Core.Executioning.Debugging
{
    public class DialogCodeStructure(IList<INode> entries, IList<DialogCodeStructureSection> sections, IDictionary<DialogCodeStructureSection, DialogCodeStructureSection> connections)
    {
        public ReadOnlyCollection<INode> Entries { get; } = new(entries);
        public ReadOnlyCollection<DialogCodeStructureSection> Sections { get; } = new(sections);
        public ReadOnlyDictionary<DialogCodeStructureSection, DialogCodeStructureSection> Connections { get; } = new(connections);

        #region Статика

        public static DialogCodeStructure Create(IList<INode> entries)
        {
            List<DialogCodeStructureSection> sections = [];
            Dictionary<DialogCodeStructureSection, DialogCodeStructureSection> connections = [];

            foreach (var entry in entries)
            {
                sections.Add(DialogCodeStructureSection.Create(entry));
            }

            return new(entries, sections, connections);
        }

        #endregion
    }
}
