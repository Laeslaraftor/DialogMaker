namespace DialogMaker.Core
{
    public class DialogProjectCharacter : ObservableObject
    {
        public DialogProjectCharacter(string id)
        {
            Id = id;
        }

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

        private string _name = string.Empty;
    }
}
