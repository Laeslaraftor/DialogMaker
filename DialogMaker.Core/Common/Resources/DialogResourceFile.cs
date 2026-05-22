using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Ресурс файла
    /// </summary>
    public class DialogResourceFile : DialogResourceObject, IResourceFile
    {
        /// <summary>
        /// Создать новый экземпляр ресурса файла
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="item">Ресурс файла на основе которого будет создан новый экземпляр</param>
        public DialogResourceFile(DialogResources resources, DialogProjectItem item) : base(resources, item)
        {
            FileName = item.FileName;
            FilePath = Path.Combine(resources.Folder, FileName);
            Type = item.Type;
        }
        /// <summary>
        /// Создать новый экземпляр ресурса файла
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="savedState">Сохранённое состояние ресурса</param>
        public DialogResourceFile(DialogResources resources, DialogResourceFileSavedState savedState) : base(resources, savedState)
        {
            FileName = savedState.FileName;
            FilePath = Path.Combine(resources.Folder, FileName);
            Type = savedState.Type;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override DialogResourceType ResourceType => DialogResourceType.File;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DialogFileResourceType Type { get; }
        /// <summary>
        /// Название файла ресурса
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string FilePath { get; }

        #region Управление

        /// <summary>
        /// Получить содержимое файла
        /// </summary>
        /// <returns></returns>
        public byte[] GetContent()
        {
            return File.ReadAllBytes(FilePath);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"[{Type}] {FilePath}";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            return new DialogResourceFileSavedState()
            {
                FileName = FileName,
                Type = Type
            };
        }

        #endregion
    }
}
