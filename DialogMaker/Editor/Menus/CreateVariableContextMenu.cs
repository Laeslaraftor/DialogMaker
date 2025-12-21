namespace DialogMaker.Editor.Menus
{
    public class CreateVariableContextMenu : TypeContextMenu<ProjectResources>
    {
        public CreateVariableContextMenu()
        {
        }
        public CreateVariableContextMenu(ProjectResources item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            if (Item == null)
            {
                yield break;
            }

            foreach (var info in ProjectVariable.TypeNames)
            {
                yield return new ContextMenuAction(info.Value, Item.CreateVariableCommand)
                {
                    CommandParameter = info.Key
                };
            }
        }

        #region Команды

        #endregion

        #region Статика

        public static readonly CreateVariableContextMenu Instance = new();

        #endregion
    }
}
