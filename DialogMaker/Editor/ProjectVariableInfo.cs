using System.Windows.Input;

namespace DialogMaker.Editor
{
    public readonly struct ProjectVariableInfo(string name, ICommand createCommand, object? createCommandParameter = null)
    {
        public string Name { get; } = name;
        public ICommand CreateCommand { get; } = createCommand;
        public object? CreateCommandParameter { get; } = createCommandParameter;
    }
}
