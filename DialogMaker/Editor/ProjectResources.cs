using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Filters;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectResources : Disposable
    {
        public ProjectResources(ProjectController controller, DialogProjectResources resources, ProjectResources? parent = null)
        {
            Controller = controller;
            Original = resources;
            Parent = parent;

            Strings = CreateObservable(new ProjectStringConverter(controller), resources.Strings, out _strings);
            Characters = CreateObservable(new ProjectCharacterConverter(controller), resources.Characters, out _characters);
            Variables = CreateObservable(new ProjectVariableConverter(controller), resources.Variables, out _variables);
            Files = CreateObservable(new ProjectFileConverter(controller), resources.Items, out _files);

            CreateStringCommand = new RelayCommand(ExecuteCreateString);
            CreateCharacterCommand = new RelayCommand(ExecuteCreateCharacter);
            CreateVariableCommand = new RelayCommand(ExecuteCreateVariable);
            AddFileCommand = new RelayCommand(p => AddFile());
            CreateVariablesContextMenu = new CreateVariableContextMenu(this);

            List<ReferenceReadOnlyList<ProjectString>> inheritStrings = [Strings];
            List<ReferenceReadOnlyList<ProjectCharacter>> inheritCharacters = [Characters];
            List<ReferenceReadOnlyList<ProjectVariable>> inheritVariables = [Variables];
            List<ReferenceReadOnlyList<ProjectFile>> inheritFiles = [Files];
            DialogResourcesFlags flags = resources.Flags;

            while (parent != null)
            {
                inheritStrings.Add(parent.Strings);
                inheritCharacters.Add(parent.Characters);
                inheritVariables.Add(parent.Variables);
                inheritFiles.Add(parent.Files);

                flags |= parent.Flags;
                parent = parent.Parent;
            }

            InheritedStrings = new(inheritStrings, controller.ResourcesFilter);
            InheritedCharacters = new(inheritCharacters, controller.ResourcesFilter);
            InheritedVariables = new(inheritVariables, controller.ResourcesFilter);
            InheritedFiles = new(inheritFiles, controller.ResourcesFilter);
            Flags = flags;
            UnsettedFlags = ProjectResourcesFilter.AllFlags & ~flags;
        }

        public ProjectController Controller { get; }
        public DialogProjectResources Original { get; }
        public DialogResourcesFlags Flags { get; }
        public DialogResourcesFlags UnsettedFlags { get; }
        public ProjectResources? Parent { get; }
        public ReferenceReadOnlyList<ProjectString> Strings { get; }
        public ReferenceReadOnlyList<ProjectCharacter> Characters { get; }
        public ReferenceReadOnlyList<ProjectVariable> Variables { get; }
        public ReferenceReadOnlyList<ProjectFile> Files { get; }
        public UnitedCollection<ReferenceReadOnlyList<ProjectString>, ProjectString> InheritedStrings { get; }
        public UnitedCollection<ReferenceReadOnlyList<ProjectCharacter>, ProjectCharacter> InheritedCharacters { get; }
        public UnitedCollection<ReferenceReadOnlyList<ProjectVariable>, ProjectVariable> InheritedVariables { get; }
        public UnitedCollection<ReferenceReadOnlyList<ProjectFile>, ProjectFile> InheritedFiles { get; }
        public ContextMenu CreateVariablesContextMenu { get; }
        public ICommand CreateStringCommand { get; }
        public ICommand CreateCharacterCommand { get; }
        public ICommand CreateVariableCommand { get; }
        public ICommand AddFileCommand { get; }

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

        #region Статика

        private static ReferenceReadOnlyList<T> CreateObservable<TOriginal, T>(IValueConverter<TOriginal, T> converter, EditableCollection<TOriginal> items, out CollectionSynchronizer<TOriginal, T> sync)
                where T : ProjectResourceItem<TOriginal>
                where TOriginal : DialogProjectResourceObject
        {
            ObservableList<T> list = [];
            sync = new(items, list, converter);

            return new(list);
        }

        #endregion
    }
}
