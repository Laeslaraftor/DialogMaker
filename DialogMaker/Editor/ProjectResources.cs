using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using Microsoft.Win32;
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
            _filesConverter = new(controller);

            ObservableList<ProjectString> strings = [];
            _strings = new(resources.Strings, strings, _stringsConverter);
            Strings = new(strings);

            ObservableList<ProjectCharacter> characters = [];
            _characters = new(resources.Characters, characters, _charactersConverter);
            Characters = new(characters);

            ObservableList<ProjectVariable> variables = [];
            _variables = new(resources.Variables, variables, _variablesConverter);
            Variables = new(variables);

            ObservableList<ProjectFile> files = [];
            _files = new(resources.Items, files, _filesConverter);
            Files = new(files);

            CreateStringCommand = new RelayCommand(ExecuteCreateString);
            CreateCharacterCommand = new RelayCommand(ExecuteCreateCharacter);
            CreateVariableCommand = new RelayCommand(ExecuteCreateVariable);
            AddFileCommand = new RelayCommand(p => AddFile());
            CreateVariablesContextMenu = new CreateVariableContextMenu(this);
        }

        public ProjectController Controller { get; }
        public DialogProjectResources Original { get; }
        public ReferenceReadOnlyList<ProjectString> Strings { get; }
        public ReferenceReadOnlyList<ProjectCharacter> Characters { get; }
        public ReferenceReadOnlyList<ProjectVariable> Variables { get; }
        public ReferenceReadOnlyList<ProjectFile> Files { get; }
        public ContextMenu CreateVariablesContextMenu { get; }
        public ICommand CreateStringCommand { get; }
        public ICommand CreateCharacterCommand { get; }
        public ICommand CreateVariableCommand { get; }
        public ICommand AddFileCommand { get; }

        private readonly ProjectStringConverter _stringsConverter;
        private readonly ProjectCharacterConverter _charactersConverter;
        private readonly ProjectVariableConverter _variablesConverter;
        private readonly ProjectFileConverter _filesConverter;
        private readonly CollectionSynchronizer<DialogProjectString, ProjectString> _strings;
        private readonly CollectionSynchronizer<DialogProjectCharacter, ProjectCharacter> _characters;
        private readonly CollectionSynchronizer<DialogProjectVariable, ProjectVariable> _variables;
        private readonly CollectionSynchronizer<DialogProjectItem, ProjectFile> _files;

        #region Управление

        public void AddFile()
        {
            OpenFileDialog dialog = new()
            {
                Multiselect = true,
                Filter = DialogProjectItem.FilesFilter
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            foreach (var filePath in dialog.FileNames)
            {
                try
                {
                    Original.AddItem(filePath);
                }
                catch (Exception error)
                {
                    error.Alert();
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _strings.Dispose();
            _characters.Dispose();
            _variables.Dispose();
            _files.Dispose();
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
