using DialogMaker.Core.Editor;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Common
{
    public class DialogResources : Disposable, IResourcesContainer
    {
        public DialogResources(IDialogResourcesContainer container, DialogProjectResources resources)
        {
            Container = container;
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


        #endregion
    }
}
