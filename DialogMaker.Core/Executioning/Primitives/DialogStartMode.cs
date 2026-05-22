namespace DialogMaker.Core.Executioning
{
    /// <summary>
    /// Режим запуска диалога
    /// </summary>
    public enum DialogStartMode
    {
        /// <summary>
        /// Стандартный запуск
        /// </summary>
        Default,
        /// <summary>
        /// Запуск в изолированном режиме.
        /// В этом режиме не сохраняются изменения в ресурсах, которые вносит диалог
        /// </summary>
        Isolated
    }
}
