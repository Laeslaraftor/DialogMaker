using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using System;

namespace DialogMaker.Core
{
    [Flags]
    public enum AllowedObjectValues
    {
        [Name("Число"), Type(typeof(float), 0f), Type(typeof(double), 0d), Type(typeof(int), 0)]
        Number = 1 << 1,
        [Name("Строка"), Type(typeof(string), "")]
        String = 1 << 2,
        [Name("Переключатель"), Type(typeof(bool), false)]
        Bool = 1 << 3,
        [Name("Ресурс"), Type(typeof(DialogProjectReference), null!)]
        Resource = 1 << 4,
        [Name("Список"), Type(typeof(EditableCollection<object>), null!)]
        List = 1 << 5,
        AllWithoutList = Number | String | Bool | Resource,
        All = AllWithoutList | List
    }
}
