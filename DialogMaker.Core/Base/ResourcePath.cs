using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    public readonly struct ResourcePath : IEquatable<ResourcePath>
    {
        public ResourcePath(DialogResourceType type, string id, string ownerPath)
        {
            ResourceType = type;
            Id = id;
            OwnerPath = ownerPath;
        }
        public ResourcePath(ResourcePath path, string id)
            : this(path.ResourceType, id, path.OwnerPath)
        {
        }

        public DialogResourceType ResourceType { get; }
        public string Id { get; }
        public string OwnerPath { get; }
        public bool IsEmpty => string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(OwnerPath);

        #region Управление

        public override bool Equals(object obj)
        {
            return obj is ResourcePath path &&
                   Equals(path);
        }
        public bool Equals(ResourcePath other)
        {
            return ResourceType == other.ResourceType &&
                   Id == other.Id &&
                   OwnerPath == other.OwnerPath &&
                   IsEmpty == other.IsEmpty;
        }
        public override string ToString()
        {
            return $"{ResourceType}:{Id}:{OwnerPath}";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ResourceType, Id, OwnerPath);
        }

        #endregion

        #region Операторы

        public static implicit operator string(ResourcePath path)
        {
            return path.ToString();
        }
        public static explicit operator ResourcePath(string path)
        {
            return Parse(path);
        }

        public static bool operator ==(ResourcePath p1, ResourcePath p2) => p1.Equals(p2);
        public static bool operator !=(ResourcePath p1, ResourcePath p2) => !p1.Equals(p2);

        #endregion

        #region Статика

        public static readonly ResourcePath Empty = new();

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

        public static ResourcePath CreatePath(IResource resource)
        {
            return CreatePath(resource, o => o.Id);
        }
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
