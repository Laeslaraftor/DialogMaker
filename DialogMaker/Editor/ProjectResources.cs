using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Converters;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectResources : Disposable
    {
        public ProjectResources(ProjectController controller, DialogProjectResources resources)
        {
            Controller = controller;
            Original = resources;

            _stringsConverter = new(controller);
            _charactersConverter = new(controller);
            _variablesConverter = new(controller);

            ObservableList<ProjectString> strings = [];
            _strings = new(resources.Strings, strings, _stringsConverter);
            Strings = new(strings);

            ObservableList<ProjectCharacter> characters = [];
            _characters = new(resources.Characters, characters, _charactersConverter);
            Characters = new(characters);

            ObservableList<ProjectVariable> variables = [];
            _variables = new(resources.Variables, variables, _variablesConverter);
            Variables = new(variables);

            CreateStringCommand = new RelayCommand(ExecuteCreateString);
            CreateCharacterCommand = new RelayCommand(ExecuteCreateCharacter);
            CreateVariableCommand = new RelayCommand(ExecuteCreateVariable);
            CreateVariablesContextMenu = new CreateVariableContextMenu(this);
        }

        public ProjectController Controller { get; }
        public DialogProjectResources Original { get; }
        public ReferenceReadOnlyList<ProjectString> Strings { get; }
        public ReferenceReadOnlyList<ProjectCharacter> Characters { get; }
        public ReferenceReadOnlyList<ProjectVariable> Variables { get; }
        public ContextMenu CreateVariablesContextMenu { get; }
        public ICommand CreateStringCommand { get; }
        public ICommand CreateCharacterCommand { get; }
        public ICommand CreateVariableCommand { get; }

        private readonly ProjectStringConverter _stringsConverter;
        private readonly ProjectCharacterConverter _charactersConverter;
        private readonly ProjectVariableConverter _variablesConverter;
        private readonly CollectionSynchronizer<DialogProjectString, ProjectString> _strings;
        private readonly CollectionSynchronizer<DialogProjectCharacter, ProjectCharacter> _characters;
        private readonly CollectionSynchronizer<DialogProjectVariable, ProjectVariable> _variables;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _strings.Dispose();
            _characters.Dispose();
            _variables.Dispose();
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
        private void ExecuteCreateVariable(object? parameter)
        {
            if (parameter is not DialogVariableType type)
            {
                return;
            }

            try
            {
                Original.CreateVariable(type);
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion
    }
}
