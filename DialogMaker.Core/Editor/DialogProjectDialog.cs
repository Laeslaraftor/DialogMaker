using Acly;

namespace DialogMaker.Core
{
    public class DialogProjectDialog : ObservableObject
    {
        public DialogProjectDialog(DialogProjectPack pack, string id)
        {
            Pack = pack;
            Id = id;
            _nodes = new();
            _characters = new();
            Nodes = new(_nodes);
            Characters = new(_characters);
        }

        public DialogProjectPack Pack { get; }
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
        public ReferenceReadOnlyDictionary<string, DialogProjectCharacter> Characters { get; }

        private readonly ObservableList<DialogProjectDialogNode> _nodes;
        private readonly ObservableDictionary<string, DialogProjectCharacter> _characters;
        private string _name = string.Empty;
    }
}
