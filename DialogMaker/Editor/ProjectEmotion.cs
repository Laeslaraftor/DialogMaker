using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib.Elements;

namespace DialogMaker.Editor
{
    public class ProjectEmotion : ProjectResourceItem<DialogProjectEmotion>
    {
        public ProjectEmotion(ProjectController project, DialogProjectEmotion original) : base(project, original)
        {
        }

        private readonly ElementsPool<EmotionPreview> _previewsPool = new();

        #region Управление

        public override object? GetPreview()
        {
            var view = _previewsPool.GetElement();
            view.Emotion = Original;

            return view;
        }
        public override void FreePreview(object? preview)
        {
            if (preview is EmotionPreview view &&
                _previewsPool.Free(view))
            {
                view.Emotion = null;
            }
        }

        public override ItemContextMenu CreateContextMenu()
        {
            return new EmotionContextMenu(this);
        }

        #endregion
    }
}
