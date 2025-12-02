using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectResources : IDisposable
    {
        public ProjectResources(ProjectController controller, DialogProjectResources resources)
        {
            Controller = controller;
            Original = resources;

            _stringsConverter = new(controller);
            _charactersConverter = new(controller);

            ObservableList<ProjectString> strings = [];
            _strings = new(resources.Strings, strings, _stringsConverter);
            Strings = new(strings);

            ObservableList<ProjectCharacter> characters = [];
            _characters = new(resources.Characters, characters, _charactersConverter);
            Characters = new(characters);

            CreateStringCommand = new RelayCommand(ExecuteCreateString);
            CreateCharacterCommand = new RelayCommand(ExecuteCreateCharacter);
        }
        ~ProjectResources()
        {
            Dispose();
        }

        public ProjectController Controller { get; }
        public DialogProjectResources Original { get; }
        public ReferenceReadOnlyList<ProjectString> Strings { get; }
        public ReferenceReadOnlyList<ProjectCharacter> Characters { get; }
        public ICommand CreateStringCommand { get; }
        public ICommand CreateCharacterCommand { get; }

        private readonly ProjectStringConverter _stringsConverter;
        private readonly ProjectCharacterConverter _charactersConverter;
        private readonly CollectionSynchronizer<DialogProjectString, ProjectString> _strings;
        private readonly CollectionSynchronizer<DialogProjectCharacter, ProjectCharacter> _characters;

        #region Управление

        public void Dispose()
        {
            _strings.Dispose();
            _characters.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Команды

        private void ExecuteCreateString(object? parameter)
        {
            try
            {
                Original.CreateString();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }
        private void ExecuteCreateCharacter(object? parameter)
        {
            try
            {
                Original.CreateCharacter();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion
    }
}
