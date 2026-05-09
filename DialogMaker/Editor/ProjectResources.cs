using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Filters;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
            Emotions = CreateObservable(new ProjectEmotionConverter(controller), resources.Emotions, out _emotions);
            TriggerPresets = CreateObservable(new ProjectTriggerPresetConverter(controller), resources.TriggerPresets, out _triggerPresets);

            CreateStringCommand = new RelayCommand(ExecuteCreateString);
            CreateCharacterCommand = new RelayCommand(ExecuteCreateCharacter);
            CreateVariableCommand = new RelayCommand(ExecuteCreateVariable);
            CreateEmotionCommand = new RelayCommand(ExecuteCreateEmotion);
            CreateTriggerPresetCommand = new RelayCommand(ExecuteCreateTriggerPreset);
            AddFileCommand = new RelayCommand(p => AddFile());
            CreateVariablesContextMenu = new CreateVariableContextMenu(this);

            List<ReferenceReadOnlyList<ProjectString>> inheritStrings = [Strings];
            List<ReferenceReadOnlyList<ProjectCharacter>> inheritCharacters = [Characters];
            List<ReferenceReadOnlyList<ProjectVariable>> inheritVariables = [Variables];
            List<ReferenceReadOnlyList<ProjectFile>> inheritFiles = [Files];
            List<ReferenceReadOnlyList<ProjectEmotion>> inheritEmotions = [Emotions];
            List<ReferenceReadOnlyList<ProjectTriggerPreset>> inheritTriggerPresets = [TriggerPresets];
            DialogResourcesFlags flags = resources.Flags;

            while (parent != null)
            {
                inheritStrings.Add(parent.Strings);
                inheritCharacters.Add(parent.Characters);
                inheritVariables.Add(parent.Variables);
                inheritFiles.Add(parent.Files);
                inheritEmotions.Add(parent.Emotions);
                inheritTriggerPresets.Add(parent.TriggerPresets);

                flags |= parent.Flags;
                parent = parent.Parent;
            }

            InheritedStrings = new(inheritStrings, controller.ResourcesFilter);
            InheritedCharacters = new(inheritCharacters, controller.ResourcesFilter);
            InheritedVariables = new(inheritVariables, controller.ResourcesFilter);
            InheritedFiles = new(inheritFiles, controller.ResourcesFilter);
            InheritedEmotions = new(inheritEmotions, controller.ResourcesFilter);
            InheritedTriggerPresets = new(inheritTriggerPresets, controller.ResourcesFilter);
            Flags = flags;
            UnsettedFlags = ProjectResourcesFilter.AllFlags & ~flags;

            Original.PropertyChanging += OnOriginalPropertyChanging;
            Original.PropertyChanged += OnOriginalPropertyChanged;
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
        public ReferenceReadOnlyList<ProjectEmotion> Emotions { get; }
        public ReferenceReadOnlyList<ProjectTriggerPreset> TriggerPresets { get; }
        public Lib.UnitedCollection<ReferenceReadOnlyList<ProjectString>, ProjectString> InheritedStrings { get; }
        public Lib.UnitedCollection<ReferenceReadOnlyList<ProjectCharacter>, ProjectCharacter> InheritedCharacters { get; }
        public Lib.UnitedCollection<ReferenceReadOnlyList<ProjectVariable>, ProjectVariable> InheritedVariables { get; }
        public Lib.UnitedCollection<ReferenceReadOnlyList<ProjectFile>, ProjectFile> InheritedFiles { get; }
        public Lib.UnitedCollection<ReferenceReadOnlyList<ProjectEmotion>, ProjectEmotion> InheritedEmotions { get; }
        public Lib.UnitedCollection<ReferenceReadOnlyList<ProjectTriggerPreset>, ProjectTriggerPreset> InheritedTriggerPresets { get; }
        public ContextMenu CreateVariablesContextMenu { get; }
        public ICommand CreateStringCommand { get; }
        public ICommand CreateCharacterCommand { get; }
        public ICommand CreateVariableCommand { get; }
        public ICommand CreateEmotionCommand { get; }
        public ICommand CreateTriggerPresetCommand { get; }
        public ICommand AddFileCommand { get; }
        public string? Description
        {
            get
            {
                try
                {
                    return Original.Description;
                }
                catch (Exception error)
                {
                    error.Log();
                }

                return null;
            }
            set
            {
                try
                {
                    Original.Description = value;
                }
                catch (Exception error)
                {
                    error.Log();
                }
            }
        }

        private readonly CollectionSynchronizer<DialogProjectString, ProjectString> _strings;
        private readonly CollectionSynchronizer<DialogProjectCharacter, ProjectCharacter> _characters;
        private readonly CollectionSynchronizer<DialogProjectVariable, ProjectVariable> _variables;
        private readonly CollectionSynchronizer<DialogProjectItem, ProjectFile> _files;
        private readonly CollectionSynchronizer<DialogProjectEmotion, ProjectEmotion> _emotions;
        private readonly CollectionSynchronizer<DialogProjectTriggerPreset, ProjectTriggerPreset> _triggerPresets;

        #region Управление

        public bool TryFindByFlags(DialogResourcesFlags flags, [NotNullWhen(true)] out ProjectResources? result)
        {
            result = null;
            ProjectResources? parent = this;
            long lastFlagsValue = long.MaxValue;

            while (parent != null)
            {
                long value = (long)parent.Flags;

                if (parent.Flags.HasFlag(flags) &&
                    lastFlagsValue > value)
                {
                    result = parent;
                    lastFlagsValue = value;
                }

                parent = parent.Parent;
            }

            return result != null;
        }

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
                    error.Log();
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Original.PropertyChanging -= OnOriginalPropertyChanging;
            Original.PropertyChanged -= OnOriginalPropertyChanged;

            _strings.Dispose();
            _characters.Dispose();
            _variables.Dispose();
            _files.Dispose();
            _emotions.Dispose();
            _triggerPresets.Dispose();
        }

        #endregion

        #region Команды

        private void ExecuteCreateString(object? parameter)
        {
            TryExecute(Original.CreateString);
        }
        private void ExecuteCreateCharacter(object? parameter)
        {
            TryExecute(Original.CreateCharacter);
        }
        private void ExecuteCreateVariable(object? parameter)
        {
            if (parameter is DialogVariableType type)
            {
                TryExecute(() => Original.CreateVariable(type));
            }
        }
        private void ExecuteCreateEmotion(object? parameter)
        {
            TryExecute(Original.CreateEmotion);
        }
        private void ExecuteCreateTriggerPreset(object? parameter)
        {
            TryExecute(Original.CreateTriggerPreset);
        }

        #endregion

        #region События

        private void OnOriginalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Description))
            {
                OnPropertyChanged(e);
            }
        }
        private void OnOriginalPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(Description))
            {
                OnPropertyChanging(e);
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
