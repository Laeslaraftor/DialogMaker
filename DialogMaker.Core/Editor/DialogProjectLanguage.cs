using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectLanguage : ObservableObject, ISavable
    {
        public DialogProjectLanguage(DialogProject project)
        {
            Id = DefaultId;
            Name = DefaultName;
            Project = project;
            ProjectId = Guid.NewGuid();
        }
        public DialogProjectLanguage(DialogProject project, DialogProjectLanguageSavedState savedState)
        {
            Project = project;
            ProjectId = Guid.Parse(savedState.ProjectId);
            Id = savedState.Id;
            Name = savedState.Name;
        }

        public DialogProject Project { get; }
        public Guid ProjectId { get; }
        public string Id
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Id));
                    field = value;
                    InvokePropertyChanged(nameof(Id));
                }
            }
        }
        public string Name
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Name));
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        #region Управление

        public ISavedState Save()
        {
            return new DialogProjectLanguageSavedState
            {
                ProjectId = ProjectId.ToString(),
                Id = Id,
                Name = Name
            };
        }

        public override string ToString()
        {
            return $"{Id} - {Name}";
        }

        #endregion

        #region Константы

        public string DefaultId = "Идентификатор языка";
        public string DefaultName = "Название языка";

        #endregion
    }
}
