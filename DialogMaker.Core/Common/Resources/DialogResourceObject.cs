using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Internal;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Базовый класс ресурса диалога
    /// </summary>
    public abstract class DialogResourceObject : Disposable, IResource
    {
        /// <summary>
        /// Создать новый экземпляр ресурса диалога
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="resourceObject">Редактируемый ресурс на основе которого будет создан новый экземпляр</param>
        public DialogResourceObject(DialogResources resources, DialogProjectResourceObject resourceObject)
        {
            Id = resourceObject.Id;
            Resources = resources;
            IsSeparated = resourceObject.IsSeparated;
        }
        /// <summary>
        /// Создать новый экземпляр ресурса диалога
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="savedState">Сохранённое состояние ресурса</param>
        public DialogResourceObject(DialogResources resources, DialogResourceObjectSavedState savedState)
        {
            Id = savedState.Id;
            Resources = resources;
            IsSeparated = savedState.IsSeparated;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract DialogResourceType ResourceType { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Контейнер ресурсов, который содержит текущий ресурс
        /// </summary>
        public DialogResources Resources { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsSeparated { get; }

        IResourcesContainer IResource.Container => Resources;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public DialogItemReference CreateReference()
        {
            return DialogItemReference.CreateUnknown(this);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="InvalidOperationException">Невозможно получить путь для самостоятельного ресурса</exception>
        public ResourcePath GetPath()
        {
            if (IsSeparated)
            {
                throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
            }

            return ResourcePath.CreatePath(this);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public virtual IVariable ToVariable()
        {
            return new LocalVariable(Id);
        }

        /// <summary>
        /// Получить сохранённое состояние ресурса
        /// </summary>
        /// <returns>Сохранённое состояние ресурса</returns>
        public DialogResourceObjectSavedState Save()
        {
            DialogResourceObjectSavedState result = CreateSavedState();
            result.Id = Id;
            result.IsSeparated = IsSeparated;

            return result;
        }

        /// <summary>
        /// Создать сохранённое состояние ресурса
        /// </summary>
        /// <returns>Сохранённое состояние ресурса</returns>
        protected abstract DialogResourceObjectSavedState CreateSavedState();

        #endregion
    }
}
