using System;
using System.Collections.Generic;
using System.Data;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReference<T> : ISavable, IEquatable<DialogProjectReference<T>>
        where T : DialogProjectResourceObject
    {
        public DialogProjectReference()
        {
            Project = null;
            ItemId = Guid.Empty;
            ResourcesPath = string.Empty;
            _item = null;
        }
        private DialogProjectReference(DialogProject project, Guid id, string path)
        {
            Project = project;
            ItemId = id;
            ResourcesPath = path;
            _item = null;
        }

        public DialogProject? Project { get; }
        public Guid ItemId { get; }
        public string ResourcesPath { get; }

        private T? _item;

        #region Управление

        public T Resolve()
        {
            if (_item != null)
            {
                return _item;
            }
            if (Project == null || 
                ResourcesPath == string.Empty || 
                ItemId == Guid.Empty)
            {
                throw new InvalidOperationException("Проект, путь или идентификатор объекта пуст. Видимо, ссылка была создана неверно.");
            }

            string[] pathParts = ResourcesPath.Split('/');
            IProjectResourcesOwner? currentOwner = Project;
            int index = 0;

            if (pathParts[index] == ".")
            {
                index++;
            }
            
            while (index < pathParts.Length && 
                   currentOwner.TryGetChild(pathParts[index], out var childOwner))
            {
                currentOwner = childOwner;
                index++;
            }

            if (index + 1 < pathParts.Length || currentOwner == null)
            {
                throw new ArgumentException($"Не удалось получить владельца ресурсов с идентификатором {pathParts[index]} в {currentOwner?.Id}");
            }

            if (currentOwner.Resources.TryGetObject<T>(ItemId, out var result))
            {
                _item = result;
                return result;
            }

            throw new DataException($"Не удалось получить объект типа {typeof(T).FullName} с идентификатором {ItemId}");
        }

        public DialogProjectReferenceSavedState Save()
        {
            string? itemPath = null;

            if (ItemId != null && ResourcesPath != null)
            {
                itemPath = ToString();
            }

            return new()
            {
                ItemPath = itemPath
            };
        }

        ISavedState ISavable.Save() => Save();

        public override string ToString()
        {
            return $"{ItemId}:{ResourcesPath}";
        }
        public override bool Equals(object? obj)
        {
            return obj is DialogProjectReference<T> reference &&
                   Equals(reference);
        }
        public bool Equals(DialogProjectReference<T>? other)
        {
            return other != null &&
                   Project == other.Project &&
                   ItemId == other.ItemId &&
                   ResourcesPath == other.ResourcesPath;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Project, ItemId, ResourcesPath);
        }

        #endregion

        #region Операторы

        public static bool operator ==(DialogProjectReference<T>? r1, DialogProjectReference<T>? r2) => r1?.Equals(r2) == true;
        public static bool operator !=(DialogProjectReference<T>? r1, DialogProjectReference<T>? r2) => !(r1 == r2);

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

        public static DialogProjectReference<T> Create(DialogProjectResourceObject obj)
        {
            List<string> pathParts = new();
            IProjectResourcesOwner? current = obj.Resources.Owner;

            while (current != null)
            {
                string id = ".";

                if (current is not DialogProject)
                {
                    id = current.Id;
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

            return new(obj.Resources.Owner.Project, obj.ProjectId, path);
        }
        public static DialogProjectReference<T> Restore(DialogProject project, DialogProjectReferenceSavedState savedState)
        {
            if (savedState.ItemPath == null)
            {
                return new();
            }

            string[] parts = savedState.ItemPath.Split(':');

            if (parts.Length != 2)
            {
                throw new ArgumentException($"Недопустимый путь к ресурсу: {savedState.ItemPath}", nameof(savedState));
            }

            return new(project, Guid.Parse(parts[0]), parts[1]);
        }

        #endregion
    }
}
