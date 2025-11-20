using Acly;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialog : ObservableObject
    {
        public DialogProjectDialog(DialogProjectPack pack, string id)
        {
            Pack = pack;
            Id = id;
            Folder = Path.Combine(pack.Folder, DialogsFolder);
            _nodes = new();
            _characters = new();
            Nodes = new(_nodes);
            Characters = new(_characters);
        }
        public DialogProjectDialog(DialogProjectPack pack, DialogProjectDialogSavedState savedState)
            : this(pack, savedState.Id)
        {
            Name = savedState.Name;

            foreach (var character in savedState.Characters)
            {
                _characters.Add(new(character));
            }
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
        public ReferenceReadOnlyList<DialogProjectCharacter> Characters { get; }

        private readonly ObservableList<DialogProjectDialogNode> _nodes;
        private readonly ObservableList<DialogProjectCharacter> _characters;
        private string _name = string.Empty;

        #region Управление

        public void Save()
        {
            DialogProjectDialogSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Nodes = _nodes.Select(n => n.Save()).ToArray(),
                Characters = _characters.Select(c => c.Save()).ToArray()
            };

            string filePath = Path.Combine(Folder, $"{Id}.{JsonData.FileExtension}");

            savedState.Save(filePath);
        }

        public bool TryGetNode(Guid id, [NotNullWhen(true)] out DialogProjectDialogNode? result)
        {
            return _nodes.TryGetValue(n => n.Id == id, out result);
        }
        public bool TryGetCharacter(Guid id, [NotNullWhen(true)] out DialogProjectCharacter? result)
        {
            return _characters.TryGetValue(c => c.Id == id, out result);
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

        public DialogProjectCharacter CreateNode(string name)
        {
            DialogProjectCharacter character = new()
            {
                Name = name,
            };

            _characters.Add(character);

            return character;
        }
        public bool RemoveCharacter(DialogProjectCharacter character)
        {
            return _characters.Remove(character);
        }

        #endregion

        #region Статика

        public const string DialogsFolder = "Dialogs";

        public static DialogProjectDialog Open(string dialogFilePath)
        {
            return SavedState.Restore<DialogProjectDialog, DialogProjectDialogSavedState>(dialogFilePath);
        }

        #endregion
    }
}
