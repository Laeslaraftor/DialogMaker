using Acly;
using System;
using DialogMaker.Core.Editor;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectPack : Disposable, IProjectResourcesOwner
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
                    Dialogs.Add(projectDialog);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
        }
        private DialogProjectPack(DialogProject project, string id, bool createResources)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Идентификатор набора не должен быть пустым!", nameof(id));
            }

            Project = project;
            Id = id;
            Folder = Path.Combine(project.ProjectPath, id);

            if (createResources)
            {
                Resources = new(this, DialogResourcesFlags.Pack);
            }
            else
            {
                Resources = DialogProjectResources.OpenOrCreate(this, DialogResourcesFlags.Pack);
            }

            FileExtensions.CreateDirectory(Folder);

            Dialogs.ItemChanged += OnDialogsItemChanged;
        }

        public event EventHandler<ItemActionEventArgs<DialogProjectDialog>>? DialogsChanged;

        public DialogProject Project { get; }
        public string Folder { get; }
        public string Id { get; }
        public string Name
        {
            get => field ?? string.Empty;
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
        public EditableCollection<DialogProjectDialog> Dialogs { get; } = [];
        public DialogProjectResources Resources { get; }

        IProjectResourcesOwner? IProjectResourcesOwner.Parent => Project;
        IResourcesOwner? IResourcesOwner.Parent => Project;
        IResourcesOwner IResourcesOwner.Root => Project;
        IResourcesContainer IResourcesOwner.Resources => Resources;

        #region Управление

        public void Save()
        {
            CheckHelper.CheckIsDisposed(this);
            Resources.Save();

            foreach (var dialog in Dialogs)
            {
                dialog.Save(); 
            }

            DialogProjectPackSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Dialogs = Dialogs.Select(d => d.Id).ToArray()
            };

            string filePath = Path.Combine(Folder, FileName);

            savedState.Save(filePath);
        }

        public bool TryGetDialog(string id, [NotNullWhen(true)] out DialogProjectDialog? result)
        {
            return Dialogs.TryGetValue(d => d.Id == id, out result);
        }
        bool IResourcesOwner.TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            return Dialogs.TryGetValue(d => d.Id == id, out result);
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

            Dialogs.Add(dialog);

            return dialog;
        }
        public bool RemoveDialog(DialogProjectDialog dialog)
        {
            return Dialogs.Remove(dialog);
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Dialogs.ItemChanged -= OnDialogsItemChanged;

            foreach (var dialog in Dialogs)
            {
                dialog.Dispose();
            }

            Dialogs.Clear();
            Resources.Dispose();
        }

        #endregion

        #region События

        private void OnDialogsItemChanged(object sender, CollectionItemEventArgs<DialogProjectDialog> e)
        {
            DialogsChanged?.Invoke(this, new((ItemAction)e.Action, e.Item));
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
