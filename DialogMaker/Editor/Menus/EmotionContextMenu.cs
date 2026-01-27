using DialogMaker.Lib;
using DialogMaker.Lib.Elements;

namespace DialogMaker.Editor.Menus
{
    public class EmotionContextMenu : TypeContextMenu<ProjectEmotion>
    {
        public EmotionContextMenu()
        {
        }
        public EmotionContextMenu(ProjectEmotion item) : base(item)
        {
            _moveActions = new(item);
        }

        private readonly MoveResourceItemActions? _moveActions;

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Настроить",
                CanChange, Change, Icons.Settings);

            if (_moveActions != null)
            {
                yield return _moveActions.GetModifier();
            }

            yield return ContextMenuSeparator.Instance;
            yield return new ContextMenuAction("Удалить",
                CanExecute, RemoveEmotion, Icons.Delete);
        }

        #region Команды

        private bool CanChange(object? parameter)
        {
            bool result = false;
            bool resolved = Resolve(parameter, emotion =>
            {
                result = emotion.Original is not null;
            });

            if (resolved)
            {
                return result;
            }

            return false;
        }
        private void Change(object? parameter)
        {
            Resolve(parameter, emotion =>
            {
                FaceEditorView.Open(emotion.Original);
            });
        }

        private void RemoveEmotion(object? parameter)
        {
            Resolve(parameter, emotion =>
            {
                emotion.Original.Resources.RemoveEmotion(emotion.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly EmotionContextMenu Instance = new();

        #endregion
    }
}
