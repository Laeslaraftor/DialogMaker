using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Common
{
    public class DialogResources : Disposable, IResourcesContainer
    {
        public DialogResources(IDialogResourcesContainer container, DialogProjectResources resources)
        {
            Container = container;
            Folder = Path.Combine(container.Folder, DialogProjectResources.ResourcesFolder);
            Files = MakeDictionary(resources.Items, i => new DialogResourceFile(this, i));
            Strings = MakeDictionary(resources.Strings, i => new DialogResourceString(this, i));
            Characters = MakeDictionary(resources.Characters, i => new DialogResourceCharacter(this, i));
            Variables = MakeDictionary(resources.Variables, i => DialogResourceVariable.Create(this, i));

            foreach (var file in resources.Items)
            {
                string originalPath = Path.Combine(file.FilePath, file.FileName);
                var newFile = Files[file.Id];
                File.Copy(originalPath, newFile.FilePath, true);
            }
        }
        public DialogResources(IDialogResourcesContainer container, DialogResourcesSavedState savedState)
        {
            Container = container;
            Folder = Path.Combine(container.Folder, DialogProjectResources.ResourcesFolder);

            var files = RestoreDictionary(savedState.Files, i => new DialogResourceFile(this, i));
            var strings = RestoreDictionary(savedState.Strings, i => new DialogResourceString(this, i));
            var characters = RestoreDictionary(savedState.Characters, i => new DialogResourceCharacter(this, i));
            var variables = RestoreDictionary(savedState.Variables, i => DialogResourceVariable.Create(this, i));

            Files = new(Files);
            Strings = new(Strings);
            Characters = new(Characters);
            Variables = new(Variables);
        }

        public IDialogResourcesContainer Container { get; }
        public DialogPackage Package => Container.Package;
        public string Folder { get; }
        public ReadOnlyDictionary<string, DialogResourceFile> Files { get; }
        public ReadOnlyDictionary<string, DialogResourceString> Strings { get; }
        public ReadOnlyDictionary<string, DialogResourceCharacter> Characters { get; }
        public ReadOnlyDictionary<string, DialogResourceVariable> Variables { get; }

        IResourcesOwner IResourcesContainer.Owner => Container;

        #region Управление

        public void Save()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("Невозможно сохранить очищенные ресурсы");
            }

            DialogResourcesSavedState savedState = new()
            {
                Files = Files.Values.Select(i => (DialogResourceFileSavedState)i.Save()).ToDictionary(e => e.Id),
                Strings = Strings.Values.Select(i => (DialogResourceStringSavedState)i.Save()).ToDictionary(e => e.Id),
                Characters = Characters.Values.Select(i => (DialogResourceCharacterSavedState)i.Save()).ToDictionary(e => e.Id),
                Variables = Variables.Values.Select(i => (DialogResourceVariableSavedState)i.Save()).ToDictionary(e => e.Id),
            };

            var data = MessagePackSerializer.Serialize(savedState);
            string filePath = Path.Combine(Folder, DefaultFileName);

            FileExtensions.CreateDirectory(Folder);
            File.WriteAllBytes(filePath, data);
        }

        public bool TryGetResource(DialogResourceType type, string id, [NotNullWhen(true)] out DialogResourceObject? result)
        {
            result = null;

            if (type == DialogResourceType.File)
            {
                return Files.Values.TryGetValue(v => v.Id == id, out result);
            }
            if (type == DialogResourceType.String)
            {
                return Strings.Values.TryGetValue(v => v.Id == id, out result);
            }
            if (type == DialogResourceType.Character)
            {
                return Characters.Values.TryGetValue(v => v.Id == id, out result);
            }
            if (type == DialogResourceType.Variable)
            {
                return Variables.Values.TryGetValue(v => v.Id == id, out result);
            }

            return false;
        }
        bool IResourcesContainer.TryGetResource(DialogResourceType type, string id, [NotNullWhen(true)] out IResource? result)
        {
            result = null;

            if (TryGetResource(type, id, out DialogResourceObject? obj))
            {
                result = obj;
                return true;
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            DisposeAll(Files.Values);
            DisposeAll(Strings.Values);
            DisposeAll(Characters.Values);
            DisposeAll(Variables.Values);
        }

        #endregion

        #region Константы

        public const string DefaultFileName = "Resources.dres";

        #endregion

        #region Статика

        public static DialogResources Open(IDialogResourcesContainer container)
        {
            string filePath = Path.Combine(container.Folder, DialogProjectResources.ResourcesFolder, DefaultFileName);
            return Open(container, filePath);
        }
        public static DialogResources Open(IDialogResourcesContainer container, string filePath)
        {
            var data = File.ReadAllBytes(filePath);
            var savedState = MessagePackSerializer.Deserialize<DialogResourcesSavedState>(data);

            return new DialogResources(container, savedState);
        }

        private static ReadOnlyDictionary<string, TResource> MakeDictionary<TItem, TResource>(IEnumerable<TItem> items, Func<TItem, TResource> converter)
                where TItem : DialogProjectResourceObject
                where TResource : DialogResourceObject
        {
            Dictionary<string, TResource> result = [];

            foreach (var item in items)
            {
                result.Add(item.Id, converter(item));
            }

            return new(result);
        }
        private static Dictionary<string, TResource> RestoreDictionary<TResource, TSavedState>(Dictionary<string, TSavedState> savedStates, Func<TSavedState, TResource> fabric)
        {
            Dictionary<string, TResource> result = new(savedStates.Count);

            foreach (var info in savedStates)
            {
                result.Add(info.Key, fabric(info.Value));
            }

            return result;
        }

        #endregion
    }
}
