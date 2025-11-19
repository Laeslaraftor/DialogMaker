using Acly;
using System;
using System.IO;

namespace DialogMaker.Core
{
    public class DialogProjectPack : ObservableObject
    {
        public DialogProjectPack(DialogProject project, string id)
        {
            Project = project;
            Id = id;
            Folder = Path.Combine(project.ProjectPath, id);
            Resources = new();
            _dialogs = new();
            Dialogs = new(_dialogs);
        }

        public DialogProject Project { get; }
        public string Folder { get; }
        public string Id { get; }
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
        public ReferenceReadOnlyDictionary<string, DialogProjectDialog> Dialogs { get; }
        public DialogProjectResources Resources { get; }

        private readonly ObservableDictionary<string, DialogProjectDialog> _dialogs;
        private string _name = string.Empty;

        #region Управление

        public DialogProjectDialog CreateDialog(string id, string name)
        {
            if (_dialogs.ContainsKey(id))
            {
                throw new ArgumentException($"Диалог с идентификатором {id} уже существует.", nameof(id));
            }

            DialogProjectDialog dialog = new(this, id)
            {
                Name = name
            };

            _dialogs.Add(id, dialog);

            return dialog;
        }
        public bool RemoveDialog(DialogProjectDialog dialog)
        {
            return _dialogs.Remove(dialog.Id);
        }

        #endregion
    }
}
