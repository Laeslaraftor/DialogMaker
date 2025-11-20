using Acly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResources : ObservableObject
    {
        public DialogProjectResources(DialogProjectPack pack, string id)
        {
            Id = id;
            Pack = pack;
            MainFolder = Path.Combine(pack.Folder, ResourcesFolder);
            Folder = Path.Combine(MainFolder, id);

            _replicas = new();
            _items = new();

            Replicas = new(_replicas);
            Items = new(_items);

            FileExtensions.CreateDirectory(Folder);
        }
        public DialogProjectResources(DialogProjectPack pack, DialogProjectResourcesSavedState savedState)
            : this(pack, savedState.Id)
        {
            if (pack.Project.TryGetLanguage(savedState.Language, out var language))
            {
                Language = language;
            }

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
                    _replicas.Add(new(this, replica));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }

        public DialogProjectPack Pack { get; }
        public string Id { get; }
        public string MainFolder { get; }
        public string Folder { get; }
        public DialogProjectLanguage? Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    if (value != null)
                    {
                        foreach (var resources in Pack.Resources)
                        {
                            if (resources.Language == value)
                            {
                                throw new ArgumentException("Невозможно задать язык, так как ресурсы с таким языком уже существуют", nameof(value));
                            }
                        }
                    }

                    _language = value;
                    InvokePropertyChanged(nameof(Language));
                }
            }
        }
        public ReferenceReadOnlyList<DialogProjectReplica> Replicas { get; }
        public ReferenceReadOnlyList<DialogProjectResourceItem> Items { get; }

        private readonly ObservableList<DialogProjectReplica> _replicas;
        private readonly ObservableList<DialogProjectResourceItem> _items;
        private DialogProjectLanguage? _language;

        #region Управление

        public void Save()
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            List<DialogProjectReplicaSavedState> replicas = new(_replicas.Count);
            List<DialogProjectResourceItemSavedState> items = new(_items.Count);

            SavedState.Save(replicas, _replicas);
            SavedState.Save(items, _items);

            DialogProjectResourcesSavedState savedState = new()
            {
                Id = Id,
                Language = Language != null ? Language.Id : string.Empty,
                Replicas = replicas.ToArray(),
                Items = items.ToArray()
            };

            string filePath = Path.Combine(MainFolder, $"{Id}.{JsonData.FileExtension}");

            savedState.Save(filePath);
        }

        public bool TryGetReplica(Guid id, [NotNullWhen(true)] out DialogProjectReplica? result)
        {
            return _replicas.TryGetValue(r => r.Id == id, out result);
        }
        public bool TryGetItem(Guid id, [NotNullWhen(true)] out DialogProjectResourceItem? result)
        {
            return _items.TryGetValue(i => i.Id == id, out result);
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

            DialogProjectResourceItem item = new(type, newFilePath);

            return item;
        }
        public bool RemoveItem(DialogProjectResourceItem item)
        {
            return _items.Remove(item); 
        }

        public DialogProjectReplica CreateReplica()
        {
            DialogProjectReplica replica = new(this);
            _replicas.Add(replica);

            return replica;
        }
        public bool RemoveReplica(DialogProjectReplica replica)
        {
            return _replicas.Remove(replica);
        }

        #endregion

        #region Статика

        public const string ResourcesFolder = "Resources";

        public static DialogProjectResources Open(DialogProjectPack pack, string resourcesId)
        {
            string filePath = Path.Combine(pack.Folder, ResourcesFolder, $"{resourcesId}.{JsonData.FileExtension}");
            var savedState = SavedState.Restore<DialogProjectResourcesSavedState>(filePath);

            return new(pack, savedState);
        }

        #endregion
    }
}
