using Acly;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectPack : ObservableObject, IProjectResourcesOwner
    {
        public DialogProjectPack(DialogProject project, string id)
            : this(project, id, true)
        {
        }
        private DialogProjectPack(DialogProject project, DialogProjectPackSavedState savedState)
            : this(project, savedState.Id, false)
        {
            Name = savedState.Name;
            string dialogsPath = Path.Combine(Folder, DialogProjectDialog.DialogsFolder);

            foreach (var dialog in savedState.Dialogs)
            {
                try
                {
                    string filePath = Path.Combine(dialogsPath, $"{dialog}.{JsonData.FileExtension}");
                    var projectDialog = DialogProjectDialog.Open(this, filePath);
                    _dialogs.Add(projectDialog);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }
        private DialogProjectPack(DialogProject project, string id, bool createResources)
        {
            Project = project;
            Id = id;
            Folder = Path.Combine(project.ProjectPath, id);
            _dialogs = new();
            Dialogs = new(_dialogs);

            if (createResources)
            {
                Resources = new(this);
            }
            else
            {
                Resources = DialogProjectResources.OpenOrCreate(this);
            }

            FileExtensions.CreateDirectory(Folder);
        }

        public event EventHandler<ItemActionEventArgs<DialogProjectDialog>>? DialogsChanged;

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
        public ReferenceReadOnlyList<DialogProjectDialog> Dialogs { get; }
        public DialogProjectResources Resources { get; }

        private readonly ObservableList<DialogProjectDialog> _dialogs;
        private string _name = string.Empty;

        #region Управление

        public void Save()
        {
            Resources.Save();

            foreach (var dialog in _dialogs)
            {
                dialog.Save(); 
            }

            DialogProjectPackSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Dialogs = _dialogs.Select(d => d.Id).ToArray()
            };

            string filePath = Path.Combine(Folder, FileName);

            savedState.Save(filePath);
        }

        public bool TryGetDialog(string id, [NotNullWhen(true)] out DialogProjectDialog? result)
        {
            return _dialogs.TryGetValue(d => d.Id == id, out result);
        }

        public DialogProjectDialog CreateDialog(string id, string name)
        {
            if (TryGetDialog(id, out _))
            {
                throw new ArgumentException($"Диалог с идентификатором {id} уже существует.", nameof(id));
            }

            DialogProjectDialog dialog = new(this, id)
            {
                Name = name
            };

            _dialogs.Add(dialog);

            DialogsChanged?.Invoke(this, new(ItemAction.Add, dialog));

            return dialog;
        }
        public bool RemoveDialog(DialogProjectDialog dialog)
        {
            if (_dialogs.Remove(dialog))
            {
                DialogsChanged?.Invoke(this, new(ItemAction.Remove, dialog));
                return true;
            }

            return false;
        }

        #endregion

        #region Статика

        public const string FileName = "DialogsPack.json";
            
        public static DialogProjectPack Open(DialogProject project, string packFolder)
        {
            string filePath = Path.Combine(packFolder, FileName);
            return SavedState.Restore<DialogProjectPack, DialogProjectPackSavedState>(filePath, s => new(project, s));            
        }

        #endregion
    }
}
