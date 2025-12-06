using Acly;
using DialogMaker.Core.Editor.Nodes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialog : ObservableObject, IProjectResourcesOwner
    {
        public DialogProjectDialog(DialogProjectPack pack, string id)
            : this(pack, id, true)
        {
        }
        public DialogProjectDialog(DialogProjectPack pack, DialogProjectDialogSavedState savedState)
            : this(pack, savedState.Id, false)
        {
            Name = savedState.Name;

            foreach (var node in savedState.Nodes)
            {
                try
                {
                    var restoredNode = DialogProjectDialogNode.Restore(this, node);
                    _nodes.Add(restoredNode);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error); 
                }
            }
        }
        private DialogProjectDialog(DialogProjectPack pack, string id, bool createResources)
        {
            Pack = pack;
            Id = id;
            Folder = Path.Combine(pack.Folder, DialogsFolder);
            _nodes = new();
            Nodes = new(_nodes);

            if (createResources)
            {
                Resources = new(this);
            }
            else
            {
                Resources = DialogProjectResources.OpenOrCreate(this);
            }
        }

        public DialogProject Project => Pack.Project;
        public DialogProjectPack Pack { get; }
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
        public ReferenceReadOnlyList<DialogProjectDialogNode> Nodes { get; }
        public DialogProjectResources Resources { get; }

        IProjectResourcesOwner? IProjectResourcesOwner.Parent => Pack;

        private readonly ObservableList<DialogProjectDialogNode> _nodes;
        private string _name = string.Empty;

        #region Управление

        public void Save()
        {
            Resources.Save();

            DialogProjectDialogSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Nodes = _nodes.Select(n => n.Save()).ToArray()
            };

            FileExtensions.CreateDirectory(Folder);
            string filePath = Path.Combine(Folder, $"{Id}.{JsonData.FileExtension}");

            savedState.Save(filePath);
        }

        public bool TryGetNode(Guid id, [NotNullWhen(true)] out DialogProjectDialogNode? result)
        {
            return _nodes.TryGetValue(n => n.Id == id, out result);
        }
        bool IProjectResourcesOwner.TryGetChild(string id, [NotNullWhen(true)] out IProjectResourcesOwner? result)
        {
            result = null;
            return false;
        }

        public DialogProjectDialogNode CreateNode(DialogNodeType type)
        {
            var node = DialogProjectDialogNode.Create(this, type);
            _nodes.Add(node);

            return node;
        }
        public bool RemoveNode(DialogProjectDialogNode node)
        {
            return _nodes.Remove(node);
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }

        #endregion

        #region Статика

        public const string DialogsFolder = "Dialogs";

        public static DialogProjectDialog Open(DialogProjectPack pack, string dialogFilePath)
        {
            var savedState = SavedState.Restore<DialogProjectDialogSavedState>(dialogFilePath);
            return new(pack, savedState);
        }

        #endregion
    }
}
