using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Editor
{
    public class ProjectNodeConverter(ProjectDialog dialog) : IValueConverter<DialogProjectDialogNode, DialogProjectNode>
    {
        private readonly ProjectDialog _dialog = dialog;

        public DialogProjectNode Convert(DialogProjectDialogNode Value)
        {
            return new(_dialog, Value);
        }
        public DialogProjectDialogNode ConvertBack(DialogProjectNode Value)
        {
            Value.Dispose();
            return Value.Original;
        }
    }
}
