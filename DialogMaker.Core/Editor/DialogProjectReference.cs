using System;
using System.Collections.Generic;
using System.Data;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReference : ISavable
    {
        public DialogProjectReference()
        {
            Project = null;
            ItemId = Guid.Empty;
            ResourcesPath = string.Empty;
        }
        protected DialogProjectReference(DialogProject project, Guid id, string path, DialogResourceType type)
        {
            Project = project;
            ItemId = id;
            ResourcesPath = path;
            ResourceType = type;
            _type = DialogProjectResourceObject.GetType(type, true);
        }

        public DialogProject? Project { get; }
        public Guid ItemId { get; }
        public string ResourcesPath { get; }
        public DialogResourceType ResourceType { get; }

        private readonly Type? _type;
        private DialogProjectResourceObject? _item;

        #region Управление

        public DialogProjectResourceObject Resolve()
        {
            if (_item != null)
            {
                return _item;
            }
            if (Project == null ||
                ResourcesPath == string.Empty ||
                ItemId == Guid.Empty ||
                _type == null)
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

            if (currentOwner.Resources.TryGetObject(ItemId, _type, out var result))
            {
                _item = result;
                return result;
            }

            throw new DataException($"Не удалось получить объект типа {_type.FullName} с идентификатором {ItemId}");
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

        public override bool Equals(object obj)
        {
            bool CompareReference(DialogProjectReference reference)
            {
                return ResourceType == reference.ResourceType &&
                       ItemId == reference.ItemId &&
                       ResourcesPath == reference.ResourcesPath;
            }

            if (obj is DialogProjectReference reference)
            {
                return CompareReference(reference);
            }
            if (obj is DialogProjectResourceObject resource)
            {
                return ItemId == resource.ProjectId;
            }
            if (obj is IModelContainer<DialogProjectReference> referenceContainer)
            {
                return CompareReference(referenceContainer.Model);
            }
            if (obj is IModelContainer<DialogProjectResourceObject> resourceContainer)
            {
                return ItemId == resourceContainer.Model.ProjectId;
            }

            return obj?.Equals(this) == true;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ResourceType, ItemId, ResourcesPath);
        }
        public override string ToString()
        {
            return $"{ResourceType}:{ItemId}:{ResourcesPath}";
        }

        #endregion

        #region Операторы

        public static implicit operator DialogProjectResourceObject(DialogProjectReference reference)
        {
            return reference.Resolve();
        }
        public static implicit operator DialogProjectReference(DialogProjectResourceObject obj)
        {
            return Create(obj);
        }

        #endregion

        #region Статика

        public static DialogProjectReference Create(DialogProjectResourceObject obj)
        {
            var path = GetPath(obj);
            return CreateGeneric(obj.Resources.Owner.Project, obj.ProjectId, path, obj.ResourceType);
        }
        public static DialogProjectReference Restore(DialogProject project, DialogProjectReferenceSavedState savedState)
        {
            if (savedState.ItemPath == null)
            {
                return new();
            }

            string[] parts = savedState.ItemPath.Split(':');

            if (parts.Length != 3)
            {
                throw new ArgumentException($"Недопустимый путь к ресурсу: {savedState.ItemPath}", nameof(savedState));
            }
            if (!Enum.TryParse<DialogResourceType>(parts[0], out var resourceType))
            {
                throw new ArgumentException($"Не удалось получить тип ресурса. Недопустимый путь к ресурсу: {savedState.ItemPath}", nameof(savedState));
            }

            return CreateGeneric(project, Guid.Parse(parts[1]), parts[2], resourceType);
        }

        private static string GetPath(DialogProjectResourceObject obj)
        {
            List<string> pathParts = [];
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

            return path;
        }
        private static DialogProjectReference CreateGeneric(DialogProject project, Guid id, string path, DialogResourceType type)
        {
            var resourceType = DialogProjectResourceObject.GetType(type, true);
            var openReference = typeof(DialogProjectReference<>);
            var closedReference = openReference.MakeGenericType(resourceType);

            return (DialogProjectReference)Activator.CreateInstance(closedReference, project, id, path, type);
        }

        #endregion
    }
}
