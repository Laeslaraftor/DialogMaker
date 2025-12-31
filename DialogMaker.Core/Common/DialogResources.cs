using DialogMaker.Core.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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

        #region Статика

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

        #endregion
    }
}
