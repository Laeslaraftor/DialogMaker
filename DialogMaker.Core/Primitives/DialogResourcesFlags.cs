using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    [Flags]
    public enum DialogResourcesFlags
    {
        [Name("Проект")]
        Root = 1 << 1,
        [Name("Набор")]
        Pack = 1 << 2,
        [Name("Диалог")]
        Dialog = 1 << 3,
    }
}
