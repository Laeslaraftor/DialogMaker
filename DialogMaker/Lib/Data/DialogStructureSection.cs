using DialogMaker.Core.Executioning.Debugging;
using DialogMaker.Editor;

namespace DialogMaker.Lib.Data
{
    public class DialogStructureSection(ProjectDialog dialog, DialogCodeStructure structure, DialogCodeStructureSection section)
    {
        public ProjectDialog Dialog { get; } = dialog;
        public int Index { get; } = structure.Sections.IndexOf(section);
        public DialogCodeStructure Structure { get; } = structure;
        public DialogCodeStructureSection Section { get; } = section;
    }
}
