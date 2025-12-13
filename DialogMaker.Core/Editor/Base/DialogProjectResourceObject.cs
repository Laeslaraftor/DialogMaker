using DialogMaker.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Data;

namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectResourceObject : Disposable
    {
        protected DialogProjectResourceObject(DialogProjectResources resources, Guid id)
        {
            ProjectId = id;
            Resources = resources;
        }
        protected DialogProjectResourceObject(DialogProjectResources resources)
            : this(resources, Guid.NewGuid())
        {
        }
        protected DialogProjectResourceObject(DialogProjectResources resources, DialogProjectResourceObjectSavedState savedState)
            : this(resources, Guid.Parse(savedState.ProjectId))
        {
            Id = savedState.Id;
        }

        public DialogProjectResources Resources { get; }
        public abstract DialogResourceType ResourceType { get; }
        public Guid ProjectId { get; }
        public string Id
        {
            get => _id;
            set
            {
                value = value.Trim();

                if (string.IsNullOrEmpty(value))
                {
                    value = DefaultId;
                }

                if (_id != value)
                {
                    _id = value;
                    InvokePropertyChanged(nameof(Id));
                }
            }
        }

        private string _id = DefaultId;

        #region Управление

        public ISavedState Save()
        {
            var savedState = CreateSavedState();
            savedState.ProjectId = ProjectId.ToString();
            savedState.Id = Id?.Trim() ?? DefaultId;

            return savedState;
        }

        protected abstract DialogProjectResourceObjectSavedState CreateSavedState();

        #endregion

        #region Константы

        public const string DefaultId = "Идентификатор ресурса";

        #endregion

        #region Статика

        private static readonly Dictionary<KeyValuePair<DialogResourceType, bool>, Type> _resourceTypes = [];

        public static Type GetType(DialogResourceType type, bool isDev)
        {
            KeyValuePair<DialogResourceType, bool> pair = new(type, isDev);

            if (_resourceTypes.TryGetValue(pair, out var resourceType))
            {
                return resourceType;
            }

            var types = type.GetEnumAttributes<ResourceTypeAttribute>();

            foreach (var info in types)
            {
                if (info.IsDev == isDev)
                {
                    _resourceTypes.TryAdd(pair, info.Type);
                    return info.Type;
                }
            }

            throw new ArgumentException("Не удалось получить тип ресурса", nameof(type));
        }

        #endregion
    }
}
