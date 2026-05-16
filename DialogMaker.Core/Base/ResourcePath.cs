using DialogMaker.Core.Common;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    /// <summary>
    /// Путь к ресурсу
    /// </summary>
    /// <param name="type">Тип ресурса</param>
    /// <param name="id">Идентификатор ресурса</param>
    /// <param name="ownerPath">Путь к владельцу ресурса</param>
    public readonly struct ResourcePath(DialogResourceType type, string id, string ownerPath) : IEquatable<ResourcePath>
    {
        /// <summary>
        /// Создать путь к ресурсу на основе другого пути
        /// </summary>
        /// <param name="path">Путь к ресурсу</param>
        /// <param name="id">Новый идентификатор</param>
        public ResourcePath(ResourcePath path, string id)
            : this(path.ResourceType, id, path.OwnerPath)
        {
        }

        /// <summary>
        /// Тип ресурса
        /// </summary>
        public DialogResourceType ResourceType { get; } = type;
        /// <summary>
        /// Идентификатор ресурса
        /// </summary>
        public string Id { get; } = id;
        /// <summary>
        /// Путь к владельцу ресурса
        /// </summary>
        public string OwnerPath { get; } = ownerPath;
        /// <summary>
        /// Является ли путь пустым
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(OwnerPath);

        #region Управление

        /// <summary>
        /// Найти ресурс в контейнере ресурсов
        /// </summary>
        /// <param name="owner">Контейнер ресурсов для поиска ресурса</param>
        /// <returns>Найденный ресурс</returns>
        public IResourceItem Find(IResourcesOwner owner)
        {
            return IResourcesOwner.FindResource(owner, this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override bool Equals(object? obj)
        {
            return obj is ResourcePath path &&
                   Equals(path);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Equals(ResourcePath other)
        {
            return ResourceType == other.ResourceType &&
                   Id == other.Id &&
                   OwnerPath == other.OwnerPath &&
                   IsEmpty == other.IsEmpty;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"{ResourceType}:{Id}:{OwnerPath}";
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(ResourceType, Id, OwnerPath);
        }

        #endregion

        #region Операторы

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        public static implicit operator string(ResourcePath path)
        {
            return path.ToString();
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        public static explicit operator ResourcePath(string path)
        {
            return Parse(path);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        public static bool operator ==(ResourcePath p1, ResourcePath p2) => p1.Equals(p2);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="path"><inheritdoc/></param>
        public static bool operator !=(ResourcePath p1, ResourcePath p2) => !p1.Equals(p2);

        #endregion

        #region Статика

        /// <summary>
        /// Пустой путь к ресурсу
        /// </summary>
        public static readonly ResourcePath Empty = new();

        /// <summary>
        /// Получить путь к ресурсу из строки
        /// </summary>
        /// <param name="path">Путь к ресурсу в виде строки</param>
        /// <returns>Путь к ресурсу</returns>
        /// <exception cref="ArgumentException">Путь к ресурсу должен состоять из 3 частей: типа, идентификатора и пути к ресурсу.</exception>
        /// <exception cref="ArgumentException">Не удалось получить тип объекта.</exception>
        public static ResourcePath Parse(string path)
        {
            var parts = path.Split(':');

            if (parts.Length != 3)
            {
                throw new ArgumentException($"Путь к ресурсу должен состоять из 3 частей: типа, идентификатора и пути к ресурсу.");
            }
            if (!Enum.TryParse(parts[0], out DialogResourceType type))
            {
                throw new ArgumentException($"Не удалось получить тип объекта: {parts[0]}", nameof(path));
            }

            return new(type, parts[1], parts[2]);
        }
        /// <summary>
        /// Попытаться получить путь к ресурсу из строки
        /// </summary>
        /// <param name="path">Путь к ресурсу в виде строки</param>
        /// <param name="result">Результат получения пути ресурса</param>
        /// <returns>Удалось ли получить путь к ресурсу</returns>
        public static bool TryParse(string path, [NotNullWhen(true)] out ResourcePath result)
        {
            result = default;

            var parts = path.Split(':');

            if (parts.Length != 3 ||
                !Enum.TryParse(parts[0], out DialogResourceType type))
            {
                return false;
            }

            result = new(type, parts[1], parts[2]);

            return true;
        }

        /// <summary>
        /// Создать путь к ресурсу
        /// </summary>
        /// <param name="resource">Ресурс, путь к которому надо создать</param>
        /// <returns>Путь к ресурсу</returns>
        public static ResourcePath CreatePath(IResource resource)
        {
            return CreatePath(resource, o => o.Id);
        }
        /// <summary>
        /// Создать путь к ресурсу
        /// </summary>
        /// <param name="resource">Ресурс, путь к которому надо создать</param>
        /// <param name="idSelector">Селектор идентификатора контейнера ресурсов</param>
        /// <returns>Путь к ресурсу</returns>
        public static ResourcePath CreatePath(IResource resource, Func<IResourcesOwner, string> idSelector)
        {
            List<string> pathParts = [];
            IResourcesOwner? current = resource.Container.Owner;

            while (current != null)
            {
                string id = ".";

                if (current != resource.Container.Owner.Root)
                {
                    id = idSelector(current);
                }

                pathParts.Add(id);
                current = current.Parent;
            }

            string path = string.Empty;

            for (int i = pathParts.Count - 1; i >= 0; i--)
            {
                path += pathParts[i];

                if (i > 0)
                {
                    path += '/';
                }
            }

            return new(resource.ResourceType, resource.Id, path);
        }

        #endregion
    }
}
