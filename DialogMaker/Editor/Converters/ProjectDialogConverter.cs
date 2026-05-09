using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectDialogConverter(ProjectPack pack) : IValueConverter<DialogProjectDialog, ProjectDialog>
    {
        private readonly ProjectPack _pack = pack;

        public ProjectDialog Convert(DialogProjectDialog value)
        {
            return new(_pack, value);
        }

        public DialogProjectDialog ConvertBack(ProjectDialog value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
