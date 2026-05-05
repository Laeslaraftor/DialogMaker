namespace DialogMaker.Core.Editor
{
    public class DialogProjectReference : Disposable, ISavable
    {
        protected DialogProjectReference(DialogProject project)
        {
            Project = project;
        }
        protected DialogProjectReference(DialogProject project, Guid id, ResourcePath path)
        {
            Project = project;
            ItemId = id;
            ResourcesPath = path;
            _type = DialogProjectResourceObject.GetType(path.ResourceType, true);

            project.ResourceItemPathChanged += OnProjectResourceItemPathChanged;
            project.Disposed += OnProjectDisposed;
        }

        public DialogProject Project { get; }
        public Guid ItemId { get; }
        public ResourcePath ResourcesPath
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(ResourcesPath));
                    field = value;
                    OnPropertyChanged(nameof(ResourcesPath));
                }
            }
        }
        public DialogResourceType ResourceType => ResourcesPath.ResourceType;

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

            var item = IResourcesOwner.FindResource(Project, ResourcesPath);

            if (item is not DialogProjectResourceObject obj)
            {
                throw new InvalidCastException($"Получен ресурс недопустимого типа: {item.GetType().FullName}");
            }

            _item = obj;

            return obj;
        }

        public DialogProjectReferenceSavedState Save()
        {
            CheckHelper.CheckIsDisposed(this);

            string? itemPath = null;

            if (!ResourcesPath.IsEmpty)
            {
                itemPath = ResourcesPath;
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
            return ResourcesPath;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Project.ResourceItemPathChanged -= OnProjectResourceItemPathChanged;
            Project.Disposed -= OnProjectDisposed;
        }

        #endregion

        #region События

        private void OnProjectDisposed(object sender, EventArgs e)
        {
            Dispose();
        }
        private void OnProjectResourceItemPathChanged(object sender, ResourceItemPathChangedEventArgs e)
        {
            if (e.Item.ProjectId != ItemId ||
                e.OldPath != ResourcesPath)
            {
                return;
            }

            ResourcesPath = e.NewPath;
            _item ??= e.Item;
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
            return CreateGeneric(obj.Resources.Owner.Project, obj.ProjectId, obj.Path);
        }
        public static DialogProjectReference Restore(DialogProject project, DialogProjectReferenceSavedState savedState)
        {
            if (savedState.ItemPath == null)
            {
                throw new ArgumentException("Путь к объекту не указан", nameof(savedState));
            }

            var path = ResourcePath.Parse(savedState.ItemPath);

            return CreateGeneric(project, Guid.Parse(path.Id), path);
        }

        private static DialogProjectReference CreateGeneric(DialogProject project, Guid id, ResourcePath path)
        {
            var resourceType = DialogProjectResourceObject.GetType(path.ResourceType, true);
            var openReference = typeof(DialogProjectReference<>);
            var closedReference = openReference.MakeGenericType(resourceType);

            return (DialogProjectReference)Activator.CreateInstance(closedReference, project, id, path);
        }

        #endregion
    }
}
