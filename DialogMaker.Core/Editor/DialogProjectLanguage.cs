using Newtonsoft.Json;
using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectLanguage : ObservableObject, ISavable
    {
        public DialogProjectLanguage(DialogProject project)
        {
            Project = project;
            ProjectId = Guid.NewGuid();
        }
        public DialogProjectLanguage(DialogProject project, DialogProjectLanguageSavedState savedState)
        {
            Project = project;
            ProjectId = Guid.Parse(savedState.ProjectId);
            _id = savedState.Id;
            _name = savedState.Name;
        }

        public DialogProject Project { get; }
        public Guid ProjectId { get; }
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    InvokePropertyChanged(nameof(Id));
                }
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        private string _id = string.Empty;
        private string _name = string.Empty;

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
    }
}
