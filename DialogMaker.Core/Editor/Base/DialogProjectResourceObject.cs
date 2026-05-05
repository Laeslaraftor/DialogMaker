using DialogMaker.Core.Attributes;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectResourceObject : Disposable, IResource, IEquatable<IResourceItem>, IEquatable<DialogProjectResourceObject>
    {
        protected DialogProjectResourceObject(DialogProjectResources resources, Guid id)
        {
            ProjectId = id;
            Resources = resources;
            Path = ResourcePath.CreatePath(this);

            resources.Owner.Project.Register(this);
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

        public event EventHandler<ResourceItemPathChangedEventArgs>? PathChanged;

        public DialogProjectResources Resources
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Resources));
                    field = value;
                    OnPropertyChanged(nameof(Resources));
                }
            }
        }
        public abstract DialogResourceType ResourceType { get; }
        public Guid ProjectId { get; }
        public string Id
        {
            get => _id;
            set
            {
                if (IsDisposed)
                {
                    return;
                }

                value = value.Trim();

                if (string.IsNullOrEmpty(value))
                {
                    value = DefaultId;
                }

                if (_id != value)
                {
                    OnPropertyChanging(nameof(Id));
                    _id = value;
                    bool startSwitchId = _switchItemId;

                    _switchItemId = true;
                    _finalPath = ResourcePath.CreatePath(this);

                    _switchItemId = startSwitchId;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        public ResourcePath Path
        {
            get
            {
                if (_switchItemId)
                {
                    return _finalPath;
                }

                return _projectPath;
            }
            private set
            {
                if (_projectPath != value)
                {
                    OnPropertyChanging(nameof(Path));
                    _projectPath = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }
        public bool IsSeparated => false;

        IResourcesContainer IResource.Container => Resources;
        string IResourceItem.Id
        {
            get
            {
                if (_switchItemId)
                {
                    return _id;
                }

                return ProjectId.ToString();
            }
        }

        private ResourcePath _projectPath;
        private ResourcePath _finalPath;
        private string _id = DefaultId;
        private bool _switchItemId;

        #region Управление

        public override bool Equals(object obj)
        {
            if (obj is DialogProjectResourceObject resourceObject)
            {
                return Equals(resourceObject);
            }
            if (obj is IResourceItem resource)
            {
                return Equals(resource);
            }

            return false;
        }
        public bool Equals(IResourceItem other)
        {
            return Id == other.Id &&
                   ResourceType == other.ResourceType &&
                   IsSeparated == other.IsSeparated &&
                   Path == other.GetPath();
        }
        public bool Equals(DialogProjectResourceObject other)
        {
            return ProjectId == other.ProjectId &&
                   ResourceType == other.ResourceType &&
                   IsSeparated == other.IsSeparated &&
                   Path == other.Path;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ResourceType, IsSeparated, Path);
        }

        public DialogItemReference CreateReference()
        {
            return DialogItemReference.Create(this);
        }

        DialogItemReference IResourceItem.CreateReference()
        {
            _switchItemId = true;
            var result = DialogItemReference.Create(this);
            _switchItemId = false;

            return result;
        }
        ResourcePath IResourceItem.GetPath() => Path;
        public abstract IVariable ToVariable();

        public void MoveTo(IProjectResourcesOwner resourcesOwner)
        {
            MoveTo(resourcesOwner.Resources);
        }
        public void MoveTo(DialogProjectResources resources)
        {
            if (IsDisposed || resources == Resources)
            {
                return;
            }
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            var currentResources = Resources;
            var currentPath = Path;

            if (!currentResources.RemoveItem(this))
            {
                throw new InvalidOperationException($"Не удалось удалить ресурс \"{this}\" из {currentResources}");
            }
            if (!HandleResourceMoving(currentResources, resources))
            {
                throw new InvalidOperationException($"Не удалось обработать перемещение ресурса \"{this}\" из {currentResources} в {resources}");
            }

            Resources = resources;
            Path = ResourcePath.CreatePath(this);

            resources.AddItem(this);

            PathChanged?.Invoke(this, new(this, currentResources, currentPath));
        }

        public ISavedState Save()
        {
            var savedState = CreateSavedState();
            savedState.ProjectId = ProjectId.ToString();
            savedState.Id = Id?.Trim() ?? DefaultId;

            return savedState;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Resources.Owner.Project.Unregister(this);
        }

        protected abstract DialogProjectResourceObjectSavedState CreateSavedState();

        protected virtual bool HandleResourceMoving(DialogProjectResources from, DialogProjectResources to)
        {
            return true;
        }

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
