using Acly;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectPack : ObservableObject
    {
        public DialogProjectPack(DialogProject project, string id)
        {
            Project = project;
            Id = id;
            Folder = Path.Combine(project.ProjectPath, id);
            _resources = new();
            _dialogs = new();
            Resources = new(_resources);
            Dialogs = new(_dialogs);

            FileExtensions.CreateDirectory(Folder);
        }
        private DialogProjectPack(DialogProject project, DialogProjectPackSavedState savedState)
            : this(project, savedState.Id)
        {
            Name = savedState.Name;
            string dialogsPath = Path.Combine(Folder, DialogProjectDialog.DialogsFolder);

            foreach (var resource in savedState.Resources)
            {
                try
                {
                    var projectResource = DialogProjectResources.Open(this, resource);
                    _resources.Add(projectResource);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
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
        public ReferenceReadOnlyList<DialogProjectResources> Resources { get; }

        private readonly ObservableList<DialogProjectDialog> _dialogs;
        private readonly ObservableList<DialogProjectResources> _resources;
        private string _name = string.Empty;

        #region Управление

        public void Save()
        {
            foreach (var resource in Resources)
            {
                resource.Save();
            }
            foreach (var dialog in _dialogs)
            {
                dialog.Save(); 
            }

            DialogProjectPackSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Dialogs = _dialogs.Select(d => d.Id).ToArray(),
                Resources = _resources.Select(r => r.Id).ToArray()
            };

            string filePath = Path.Combine(Folder, FileName);

            savedState.Save(filePath);
        }

        public bool TryGetDialog(string id, [NotNullWhen(true)] out DialogProjectDialog? result)
        {
            return _dialogs.TryGetValue(d => d.Id == id, out result);
        }
        public bool TryGetResources(DialogProjectLanguage language, [NotNullWhen(true)] out DialogProjectResources? result)
        {
            return _resources.TryGetValue(r => r.Language == language, out result);
        }
        public bool TryGetResources(string id, [NotNullWhen(true)] out DialogProjectResources? result)
        {
            return _resources.TryGetValue(r => r.Id == id, out result);
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

        public DialogProjectResources CreateResources(string id, DialogProjectLanguage language)
        {
            if (TryGetResources(language, out _))
            {
                throw new ArgumentException($"Невозможно создать ресурсы для языка {language}, так как ресурсы для этого языка уже существуют", nameof(language));
            }
            if (TryGetResources(id, out _))
            {
                throw new ArgumentException($"Невозможно создать ресурсы с идентификатором {id}, так как ресурсы с таким идентификатором уже существуют", nameof(language));
            }


            DialogProjectResources resources = new(this, id)
            {
                Language = language
            };

            if (!Directory.Exists(resources.Folder))
            {
                Directory.CreateDirectory(resources.Folder);
            }

            _resources.Add(resources);

            return resources;
        }
        public bool RemoveResource(DialogProjectResources resources)
        {
            return _resources.Remove(resources);
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
