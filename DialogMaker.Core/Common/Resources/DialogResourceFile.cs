using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceFile : DialogResourceObject, IResourceFile
    {
        public DialogResourceFile(DialogResources resources, DialogProjectItem item) : base(resources, item)
        {
            FileName = item.FileName;
            FilePath = Path.Combine(resources.Folder, FileName);
            Type = item.Type;
        }
        public DialogResourceFile(DialogResources resources, DialogResourceFileSavedState savedState) : base(resources, savedState)
        {
            FileName = savedState.FileName;
            FilePath = Path.Combine(resources.Folder, FileName);
            Type = savedState.Type;
        }

        public override DialogResourceType ResourceType => DialogResourceType.File;
        public DialogFileResourceType Type { get; }
        public string FileName { get; }
        public string FilePath { get; }


        #region Управление

        public byte[] GetContent()
        {
            return File.ReadAllBytes(FilePath);
        }

        public override string ToString()
        {
            return $"[{Type}] {FilePath}";
        }

        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            return new DialogResourceFileSavedState()
            {
                FileName = FileName,
                Type = Type
            };
        }

        #endregion
    }
}
