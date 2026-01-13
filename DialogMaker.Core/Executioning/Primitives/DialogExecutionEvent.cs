using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core.Executioning
{
    public enum DialogExecutionEvent : byte
    {
        [Name("Запуск")]
        Started,
        [Name("Пауза")]
        Paused,
        [Name("Возобновление")]
        Resumed,
        [Name("Завершение")]
        Completing
    }
}
