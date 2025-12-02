using DialogMaker.Core;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public class ProjectReference<T, TItem> : ObservableObject
        where T : ProjectResourceItem<TItem> where TItem : DialogProjectResourceObject
    {
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        public ProjectReference(ProjectController project, DialogProjectReference<TItem> reference)
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        {
            Project = project;
            Reference = reference;
#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
            Item = (T)Activator.CreateInstance(typeof(T), project, reference.Resolve());
#pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
        }
        public ProjectReference(ProjectController project, T item)
        {
            Project = project;
            Reference = item.Original;
            Item = item;
        }

        public ProjectController Project { get; }
        public DialogProjectReference<TItem> Reference { get; }
        public T Item { get; }

        #region Операторы

        public static implicit operator T(ProjectReference<T, TItem> reference)
        {
            return reference.Item;
        }
        public static implicit operator DialogProjectReference<TItem>(ProjectReference<T, TItem> reference)
        {
            return reference.Reference;
        }
        public static implicit operator ProjectReference<T, TItem>(T item)
        {
            return new(item.Project, item);
        }

        #endregion
    }
}
