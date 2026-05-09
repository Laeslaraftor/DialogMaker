using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Editor
{
    public class ProjectNodeConverter(ProjectDialog dialog) : IValueConverter<DialogProjectDialogNode, DialogProjectNode>
    {
        private readonly ProjectDialog _dialog = dialog;

        public DialogProjectNode Convert(DialogProjectDialogNode value)
        {
            return new(_dialog, value);
        }
        public DialogProjectDialogNode ConvertBack(DialogProjectNode value)
        {
            value.Dispose();
            return value.Original;
        }
    }
}
