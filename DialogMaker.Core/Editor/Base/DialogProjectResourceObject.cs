using System;

namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectResourceObject : ObservableObject, ISavable, IDisposable
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
        ~DialogProjectResourceObject()
        {
            Dispose(false);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract DialogProjectResourceObjectSavedState CreateSavedState();

        protected virtual void Dispose(bool isDisposing)
        {
        }

        #endregion

        #region Константы

        public const string DefaultId = "Идентификатор ресурса";

        #endregion
    }
}
