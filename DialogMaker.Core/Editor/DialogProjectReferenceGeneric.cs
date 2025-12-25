using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReference<T> : DialogProjectReference
        where T : DialogProjectResourceObject
    {
        public DialogProjectReference(DialogProject project, Guid id, string path, DialogResourceType type)
            : base(project, id, path, type)
        {
        }

        #region Управление

        public new T Resolve()
        {
            return (T)base.Resolve();
        }

        #endregion

        #region Операторы

        public static implicit operator T(DialogProjectReference<T> reference)
        {
            return reference.Resolve();
        }
        public static implicit operator DialogProjectReference<T>(T obj)
        {
            return Create(obj);
        }

        #endregion

        #region Статика

        public static new DialogProjectReference<T> Create(DialogProjectResourceObject obj)
        {
            return (DialogProjectReference<T>)DialogProjectReference.Create(obj);
        }
        public static new DialogProjectReference<T> Restore(DialogProject project, DialogProjectReferenceSavedState savedState)
        {
            return (DialogProjectReference<T>)DialogProjectReference.Restore(project, savedState);
        }

        #endregion
    }
}
