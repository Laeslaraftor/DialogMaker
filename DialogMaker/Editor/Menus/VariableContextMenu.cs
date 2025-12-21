using DialogMaker.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Editor.Menus
{
    public class VariableContextMenu : TypeContextMenu<ProjectVariable>
    {
        public VariableContextMenu()
        {
        }
        public VariableContextMenu(ProjectVariable item) : base(item)
        {
        }

        protected override IEnumerable<IContextMenuModifier> GetItems()
        {
            yield return new ContextMenuAction("Удалить переменную",
                CanExecute, RemoveString, Icons.Delete);
        }

        #region Команды

        private void RemoveString(object? parameter)
        {
            Resolve(parameter, variable =>
            {
                variable.Original.Resources.RemoveVariable(variable.Original);
            });
        }

        #endregion

        #region Статика

        public static readonly VariableContextMenu Instance = new();

        #endregion
    }
}
