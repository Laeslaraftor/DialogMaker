using Acly;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectDialogConverter(ProjectPack pack) : IValueConverter<DialogProjectDialog, ProjectDialog>
    {
        private readonly ProjectPack _pack = pack;

        public ProjectDialog Convert(DialogProjectDialog Value)
        {
            return new(_pack, Value);
        }

        public DialogProjectDialog ConvertBack(ProjectDialog Value)
        {
            Value.Dispose();
            return Value.Original;
        }
    }
}
