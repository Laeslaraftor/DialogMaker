using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    [Flags]
    public enum AllowedObjectValues
    {
        [Name("Число"), Type(typeof(float), 0f), Type(typeof(double), 0d), Type(typeof(int), 0)]
        [PortType(DialogNodePortType.Number)]
        Number = 1 << 1,
        [Name("Строка"), Type(typeof(string), "")]
        [PortType(DialogNodePortType.String)]
        String = 1 << 2,
        [Name("Переключатель"), Type(typeof(bool), false)]
        [PortType(DialogNodePortType.Bool)]
        Bool = 1 << 3,
        [Name("Ресурс"), Type(typeof(DialogProjectReference), null!)]
        [PortType(DialogNodePortType.Object)]
        Resource = 1 << 4,
        [Name("Список"), Type(typeof(EditableCollection<object>), null!)]
        [PortType(DialogNodePortType.Object)]
        List = 1 << 5,
        AllWithoutList = Number | String | Bool | Resource,
        Native = Number | String | Bool,
        All = AllWithoutList | List
    }
}
