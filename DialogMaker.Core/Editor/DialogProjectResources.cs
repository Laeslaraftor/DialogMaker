using Acly;
using System;
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
            Strings = new();
            Characters = new();

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
            foreach (var str in savedState.Strings)
            {
                try
                {
                    Strings.Add(new(this, str));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
            foreach (var character in savedState.Characters)
            {
                try
                {
                    Characters.Add(new(this, character));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }

        public IProjectResourcesOwner Owner { get; }
        public string Folder { get; }
        public EditableCollection<DialogProjectString> Strings { get; }
        public ReferenceReadOnlyList<DialogProjectItem> Items { get; }
        public EditableCollection<DialogProjectCharacter> Characters { get; }

        private readonly ObservableList<DialogProjectItem> _items;

        #region Управление

        public void Save()
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            DialogProjectResourcesSavedState savedState = new()
            {
                Strings = Strings.Select(r => (DialogProjectStringSavedState)r.Save()).ToArray(),
                Items = Items.Select(i => (DialogProjectResourceItemSavedState)i.Save()).ToArray(),
                Characters = Characters.Select(c => (DialogProjectCharacterSavedState)c.Save()).ToArray()
            };

            string filePath = Path.Combine(Folder, ResourcesFileName);

            savedState.Save(filePath);
        }

        public bool TryGetString(Guid id, [NotNullWhen(true)] out DialogProjectString? result)
        {
            return Strings.TryGetValue(r => r.ProjectId == id, out result);
        }
        public bool TryGetItem(Guid id, [NotNullWhen(true)] out DialogProjectItem? result)
        {
            return _items.TryGetValue(i => i.ProjectId == id, out result);
        }
        public bool TryGetCharacter(Guid id, [NotNullWhen(true)] out DialogProjectCharacter? result)
        {
            return Characters.TryGetValue(i => i.ProjectId == id, out result);
        }
        public bool TryGetObject<T>(Guid id, [NotNullWhen(true)] out T? result)
            where T : DialogProjectResourceObject
        {
            var requestedType = typeof(T);
            result = null;

            if (requestedType == typeof(DialogProjectString))
            {
                if (TryGetString(id, out var str))
                {
                    result = (T)Convert.ChangeType(str, requestedType);
                }
            }
            else if (requestedType == typeof(DialogProjectItem))
            {
                if (TryGetItem(id, out var item))
                {
                    result = (T)Convert.ChangeType(item, requestedType);
                }
            }
            else if (requestedType == typeof(DialogProjectCharacter))
            {
                if (TryGetCharacter(id, out var character))
                {
                    result = (T)Convert.ChangeType(character, requestedType);
                }
            }

            return result != null;
        }

        public DialogProjectItem AddItem(string filePath, bool overwrite = false)
        {
            var type = DialogProjectItem.GetResourceType(filePath);

            if (type == null)
            {
                DialogProjectItem.ThrowNotSupportedException(filePath);
            }

            string newFilePath = Path.Combine(Folder, filePath.GetFileName(false));

            if (File.Exists(newFilePath) && !overwrite)
            {
                throw new ArgumentException("Файл с таким именем уже существует.");
            }

            File.Copy(filePath, newFilePath, true);

            DialogProjectItem item = new(this, type, newFilePath);

            return item;
        }
        public bool RemoveItem(DialogProjectItem item)
        {
            return _items.Remove(item);
        }

        public DialogProjectString CreateString()
        {
            DialogProjectString str = new(this);
            Strings.Add(str);

            return str;
        }
        public bool RemoveString(DialogProjectString str)
        {
            return Strings.Remove(str);
        }

        public DialogProjectCharacter CreateCharacter()
        {
            DialogProjectCharacter character = new(this);
            Characters.Add(character);

            return character;
        }
        public bool RemoveCharacter(DialogProjectCharacter character)
        {
            return Characters.Remove(character);
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
