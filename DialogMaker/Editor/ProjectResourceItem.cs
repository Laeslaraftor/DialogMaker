using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public abstract class ProjectResourceItem<T> : ProjectResourceItem
        where T : DialogProjectResourceObject
    {
        protected ProjectResourceItem(ProjectController project, T original) : base(project, original)
        {
            Original = original;
        }

        public T Original { get; }
    }
    public abstract class ProjectResourceItem : ObservableObject, IDisposable
    {
        protected ProjectResourceItem(ProjectController project, DialogProjectResourceObject model)
        {
            Project = project;
            Model = model;

            Model.PropertyChanged += OnModelPropertyChanged;
        }
        ~ProjectResourceItem()
        {
            Dispose(false);
        }

        public ProjectController Project { get; }
        public DialogProjectResourceObject Model { get; }
        public DialogResourceType ResourceType => Model.ResourceType;
        public string Id
        {
            get => Model.Id;
            set => Model.Id = value;
        }
        public ContextMenu ContextMenu
        {
            get
            {
                field ??= CreateContextMenu();
                return field;
            }
        }
        public ICommand EditIdCommand => IdEditCommand;

        private readonly ElementsPool<TextBlock> _previewBlocks = new();
        private readonly List<TextBlock> _createdBlocks = [];

        #region Управление

        public abstract ItemContextMenu CreateContextMenu();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public override string? ToString()
        {
            return Model.ToString();
        }
        public virtual object? GetPreview()
        {
            var block = _previewBlocks.GetElement();
            block.Text = ToString() ?? string.Empty;

            _createdBlocks.Add(block);

            return block;
        }
        public virtual void FreePreview(object? preview)
        {
            if (preview is TextBlock block &&
                _createdBlocks.Remove(block))
            {
                _previewBlocks.Free(block);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
            Model.PropertyChanged -= OnModelPropertyChanged;
            _previewBlocks.Dispose();
            _createdBlocks.Clear();
        }

        #endregion

        #region События

        protected virtual void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
            InvokePropertyChanged(e.PropertyName);
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.

            string preview = ToString() ?? string.Empty;

            foreach (var block in _createdBlocks)
            {
                block.Text = preview;
            }
        }

        #endregion

        #region Статика

        public static ICommand IdEditCommand
        {
            get
            {
                field ??= new RelayCommand(ChangeIdCommand, CanExecute);
                return field;
            }
        }

        public static ProjectResourceItem Create(ProjectController controller, DialogProjectResourceObject resource)
        {
            if (resource is DialogProjectString str)
            {
                return new ProjectString(controller, str);
            }
            if (resource is DialogProjectCharacter character)
            {
                return new ProjectCharacter(controller, character);
            }
            if (resource is DialogProjectItem item)
            {
                return new ProjectResourceFile(controller, item);
            }

            throw new ArgumentException($"Неизвестный тип ресурса: {resource.GetType()}", nameof(resource));
        }

        private static bool CanExecute(object? parameter)
        {
            return (parameter is EditCommandEventArgs<string> args &&
                   args.Parameter is ProjectResourceItem) || parameter is ProjectResourceItem;
        }

        private static void ChangeIdCommand(object? parameter)
        {
            if (parameter is EditCommandEventArgs<string> args &&
                args.Parameter is ProjectResourceItem item)
            {
                item.Id = GetNotNull(args.NewValue, DialogProjectResourceObject.DefaultId);
            }
        }

        private static string GetNotNull(string? value, string falloff)
        {
            string newValue = value ?? string.Empty;
            newValue = string.IsNullOrEmpty(newValue) ? falloff : newValue;

            return newValue;
        }

        #endregion
    }
}
