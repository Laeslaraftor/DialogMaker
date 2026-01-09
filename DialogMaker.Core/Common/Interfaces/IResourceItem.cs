using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Common
{
    public interface IResourceItem
    {
        public DialogResourceType ResourceType { get; }
        public string Id { get; }
        public bool IsSeparated { get; }

        #region Управление

        public DialogItemReference CreateReference();
        public ResourcePath GetPath();
        public IVariable ToVariable();

        #endregion

        #region Константы

        public const string GetPathExceptionMessage = $"Получение пути недоступно при {nameof(IsSeparated)} установленном как True";

        #endregion
    }
}
