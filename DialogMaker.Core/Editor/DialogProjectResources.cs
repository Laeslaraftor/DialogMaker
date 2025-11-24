using Acly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResources : ObservableObject
    {
        public DialogProjectResources(IProjectResourcesOwner owner)
        {
            Owner = owner;
            Folder = Path.Combine(owner.Folder, ResourcesFolder);
            Replicas = new();

            _items = new();
            Items = new(_items);

            FileExtensions.CreateDirectory(Folder);
        }
        public DialogProjectResources(IProjectResourcesOwner owner, DialogProjectResourcesSavedState savedState)
            : this(owner)
        {
            foreach (var item in savedState.Items)
            {
                try
                {
                    _items.Add(new(this, item));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
            foreach (var replica in savedState.Replicas)
            {
                try
                {
                    Replicas.Add(new(this, replica));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }

        public IProjectResourcesOwner Owner { get; }
        public string Folder { get; }
        public EditableCollection<DialogProjectReplica> Replicas { get; }
        public ReferenceReadOnlyList<DialogProjectResourceItem> Items { get; }

        private readonly ObservableList<DialogProjectResourceItem> _items;

        #region Управление

        public void Save()
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            DialogProjectResourcesSavedState savedState = new()
            {
                Replicas = Replicas.Select(r => (DialogProjectReplicaSavedState)r.Save()).ToArray(),
                Items = Items.Select(i => (DialogProjectResourceItemSavedState)i.Save()).ToArray()
            };

            string filePath = Path.Combine(Folder, ResourcesFileName);

            savedState.Save(filePath);
        }

        public bool TryGetReplica(Guid id, [NotNullWhen(true)] out DialogProjectReplica? result)
        {
            return Replicas.TryGetValue(r => r.ProjectId == id, out result);
        }
        public bool TryGetItem(Guid id, [NotNullWhen(true)] out DialogProjectResourceItem? result)
        {
            return _items.TryGetValue(i => i.ProjectId == id, out result);
        }

        public DialogProjectResourceItem AddItem(string filePath, bool overwrite = false)
        {
            var type = DialogProjectResourceItem.GetResourceType(filePath);

            if (type == null)
            {
                DialogProjectResourceItem.ThrowNotSupportedException(filePath);
            }

            string newFilePath = Path.Combine(Folder, filePath.GetFileName(false));

            if (File.Exists(newFilePath) && !overwrite)
            {
                throw new ArgumentException("Файл с таким именем уже существует.");
            }

            File.Copy(filePath, newFilePath, true);

            DialogProjectResourceItem item = new(this, type, newFilePath);

            return item;
        }
        public bool RemoveItem(DialogProjectResourceItem item)
        {
            return _items.Remove(item); 
        }

        public DialogProjectReplica CreateReplica()
        {
            DialogProjectReplica replica = new(this);
            Replicas.Add(replica);

            return replica;
        }
        public bool RemoveReplica(DialogProjectReplica replica)
        {
            return Replicas.Remove(replica);
        }

        #endregion

        #region Статика

        public const string ResourcesFolder = "Resources";
        public const string ResourcesFileName = $"Resources.{JsonData.FileExtension}";

        public static DialogProjectResources Open(IProjectResourcesOwner owner)
        {
            string filePath = Path.Combine(owner.Folder, ResourcesFolder, ResourcesFileName);
            var savedState = SavedState.Restore<DialogProjectResourcesSavedState>(filePath);

            return new(owner, savedState);
        }
        public static DialogProjectResources OpenOrCreate(IProjectResourcesOwner owner)
        {
            try
            {
                return Open(owner);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }

            return new(owner);
        }

        #endregion
    }
}
