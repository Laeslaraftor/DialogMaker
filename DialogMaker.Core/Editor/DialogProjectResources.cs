using Acly;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResources : Disposable, IResourcesContainer
    {
        public DialogProjectResources(IProjectResourcesOwner owner, DialogResourcesFlags flags, string folderName = ResourcesFolder)
        {
            Owner = owner;
            Flags = flags;
            Folder = Path.Combine(owner.Folder, folderName);
            Strings = [];
            Characters = [];
            Variables = [];
            Items = [];
            Emotions = [];

            List<DialogProjectResources> inheritedResources = [];
            var parent = owner.Parent;

            while (parent != null)
            {
                inheritedResources.Add(parent.Resources);
                parent = parent.Parent;
            }

            InheritedResources = new(inheritedResources);

            FileExtensions.CreateDirectory(Folder);
        }
        public DialogProjectResources(IProjectResourcesOwner owner, DialogProjectResourcesSavedState savedState, DialogResourcesFlags flags, string folderName = ResourcesFolder)
            : this(owner, flags, folderName)
        {
            foreach (var item in savedState.Items)
            {
                try
                {
                    Items.Add(new(this, item));
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
            foreach (var variable in savedState.Variables)
            {
                try
                {
                    Variables.Add(DialogProjectVariable.Restore(this, variable));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
            foreach (var emotion in savedState.Emotions)
            {
                try
                {
                    Emotions.Add(new(this, emotion));
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }

        public IProjectResourcesOwner Owner { get; }
        public DialogResourcesFlags Flags { get; }
        public string Folder { get; }
        public EditableCollection<DialogProjectString> Strings { get; }
        public EditableCollection<DialogProjectItem> Items { get; }
        public EditableCollection<DialogProjectCharacter> Characters { get; }
        public EditableCollection<DialogProjectVariable> Variables { get; }
        public EditableCollection<DialogProjectEmotion> Emotions { get; }
        public ReadOnlyCollection<DialogProjectResources> InheritedResources { get; }

        IResourcesOwner IResourcesContainer.Owner => Owner;

        #region Управление

        public void Save()
        {
            CheckHelper.CheckIsDisposed(this);

            if (Owner.Project.IsDisposed || !Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            DialogProjectResourcesSavedState savedState = new()
            {
                Strings = [.. Strings.Select(r => (DialogProjectStringSavedState)r.Save())],
                Items = [.. Items.Select(i => (DialogProjectResourceItemSavedState)i.Save())],
                Characters = [.. Characters.Select(c => (DialogProjectCharacterSavedState)c.Save())],
                Variables = [.. Variables.Select(v => (DialogProjectVariableSavedState)v.Save())],
                Emotions = [.. Emotions.Select(v => (DialogProjectEmotionSavedState)v.Save())]
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
            return Items.TryGetValue(i => i.ProjectId == id, out result);
        }
        public bool TryGetCharacter(Guid id, [NotNullWhen(true)] out DialogProjectCharacter? result)
        {
            return Characters.TryGetValue(i => i.ProjectId == id, out result);
        }
        public bool TryGetVariable(Guid id, [NotNullWhen(true)] out DialogProjectVariable? result)
        {
            return Variables.TryGetValue(i => i.ProjectId == id, out result);
        }
        public bool TryGetEmotion(Guid id, [NotNullWhen(true)] out DialogProjectEmotion? result)
        {
            return Emotions.TryGetValue(i => i.ProjectId == id, out result);
        }
        public bool TryGetObject(Guid id, Type resourceType, [NotNullWhen(true)] out DialogProjectResourceObject? result)
        {
            result = null;

            if (resourceType == typeof(DialogProjectString))
            {
                if (TryGetString(id, out var str))
                {
                    result = str;
                }
            }
            else if (resourceType == typeof(DialogProjectItem))
            {
                if (TryGetItem(id, out var item))
                {
                    result = item;
                }
            }
            else if (resourceType == typeof(DialogProjectCharacter))
            {
                if (TryGetCharacter(id, out var character))
                {
                    result = character;
                }
            }
            else if (resourceType == typeof(DialogProjectEmotion))
            {
                if (TryGetEmotion(id, out var emotion))
                {
                    result = emotion;
                }
            }
            else if (typeof(DialogProjectVariable).IsAssignableFrom(resourceType))
            {
                if (TryGetVariable(id, out var variable))
                {
                    result = variable;
                }
            }

            return result != null;
        }
        public bool TryGetObject<T>(Guid id, [NotNullWhen(true)] out T? result)
            where T : DialogProjectResourceObject
        {
            result = null;

            if (TryGetObject(id, typeof(T), out var resource))
            {
                result = (T)resource;
            }

            return false;
        }
        public IEditableList GetObjectsCollection(DialogProjectResourceObject obj)
        {
            if (obj is DialogProjectString)
            {
                return Strings;
            }
            else if (obj is DialogProjectItem)
            {
                return Items;
            }
            else if (obj is DialogProjectCharacter)
            {
                return Characters;
            }
            else if (obj is DialogProjectVariable)
            {
                return Variables;
            }
            else if (obj is DialogProjectEmotion)
            {
                return Emotions;
            }

            throw new ArgumentException($"Неизвестный тип ресурса");
        }

        bool IResourcesContainer.TryGetResource(DialogResourceType type, string id, [NotNullWhen(true)] out IResource? result)
        {
            result = null;

            if (!Guid.TryParse(id, out var guidId))
            {
                return false;
            }

            var resourceType = DialogProjectResourceObject.GetType(type, true);

            if (TryGetObject(guidId, resourceType, out var resource))
            {
                result = resource;
                return true;
            }

            return false;
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
            Items.Add(item);

            return item;
        }
        public bool RemoveItem(DialogProjectItem item)
        {
            return Items.Remove(item);
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

        public DialogProjectVariable CreateVariable(DialogVariableType type)
        {
            DialogProjectVariable variable = DialogProjectVariable.Create(this, type);
            Variables.Add(variable);

            return variable;
        }
        public bool RemoveVariable(DialogProjectVariable variable)
        {
            return Variables.Remove(variable);
        }

        public DialogProjectEmotion CreateEmotion()
        {
            DialogProjectEmotion emotion = new(this);
            Emotions.Add(emotion);

            return emotion;
        }
        public bool RemoveEmotion(DialogProjectEmotion emotion)
        {
            return Emotions.Remove(emotion);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            static void Dispose<T>(IList<T> items) where T : DialogProjectResourceObject
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }

                items.Clear();
            }

            Dispose(Strings);
            Dispose(Items);
            Dispose(Characters);
            Dispose(Variables);
        }

        #endregion

        #region Перемещение

        internal bool RemoveItem(DialogProjectResourceObject obj)
        {
            if (obj.Resources != this)
            {
                return false;
            }

            GetObjectsCollection(obj).Remove(obj);

            return true;
        }
        internal bool AddItem(DialogProjectResourceObject obj)
        {
            if (obj.Resources != this)
            {
                return false;
            }

            GetObjectsCollection(obj).AddNew(obj);

            return true;
        }

        #endregion

        #region Статика

        public const string ResourcesFolder = "Resources";
        public const string ResourcesFileName = $"Resources.{JsonData.FileExtension}";

        public static DialogProjectResources Open(IProjectResourcesOwner owner, DialogResourcesFlags flags, string folderName = ResourcesFolder)
        {
            string filePath = Path.Combine(owner.Folder, folderName, ResourcesFileName);
            var savedState = SavedState.Restore<DialogProjectResourcesSavedState>(filePath);

            return new(owner, savedState, flags, folderName);
        }
        public static DialogProjectResources OpenOrCreate(IProjectResourcesOwner owner, DialogResourcesFlags flags, string folderName = ResourcesFolder)
        {
            try
            {
                return Open(owner, flags, folderName);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }

            return new(owner, flags, folderName);
        }

        #endregion
    }
}
