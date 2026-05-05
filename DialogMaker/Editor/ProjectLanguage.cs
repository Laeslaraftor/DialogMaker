using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectLanguage(ProjectController controller) : ObservableObject
    {
        public ProjectController Controller { get; } = controller;
        public DialogProjectLanguage? Language
        {
            get => field;
            set
            {
                if (field == value)
                {
                    return;
                }
                if (field != null)
                {
                    field.PropertyChanged -= OnLanguagePropertyChanged;
                }
                if (value != null)
                {
                    value.PropertyChanged += OnLanguagePropertyChanged;
                    ContextMenu = new LanguageContextMenu(this);
                }
                else
                {
                    ContextMenu = null;
                }

                field = value;
                OnPropertyChanged(nameof(Language));
            }
        }
        public DialogProject? Project => Language?.Project;
        public Guid ProjectId => Language != null ? Language.ProjectId : Guid.Empty;
        public string Id
        {
            get => Language != null ? Language.Id : string.Empty;
            set
            {
                if (Id != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        value = DefaultId;
                    }

                    Language?.Id = value;
                }
            }
        }
        public string Name
        {
            get => Language != null ? Language.Name : string.Empty;
            set
            {
                if (Name != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        value = DefaultName;
                    }

                    Language?.Name = value;
                }
            }
        }
        public ContextMenu? ContextMenu
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ContextMenu));
                }
            }
        }
        public ICommand EditIdCommand => IdEditCommand;
        public ICommand EditNameCommand => NameEditCommand;

        #region События

        private void OnLanguagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Controller?.Save();
            OnPropertyChanged(e.PropertyName ?? string.Empty);
        }

        #endregion

        #region Константы

        public const string DefaultId = "Идентификатор языка";
        public const string DefaultName = "Название языка";

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
        public static ICommand NameEditCommand
        {
            get
            {
                field ??= new RelayCommand(ChangeNameCommand, CanExecute);
                return field;
            }
        }

        private static bool CanExecute(object? parameter)
        {
            return (parameter is EditCommandEventArgs<string> args &&
                   args.Parameter is ProjectLanguage) || parameter is ProjectLanguage;
        }

        private static void ChangeIdCommand(object? parameter)
        {
            if (parameter is EditCommandEventArgs<string> args &&
                args.Parameter is ProjectLanguage language)
            {
                language.Id = GetNotNull(args.NewValue, DefaultId);
            }
        }
        private static void ChangeNameCommand(object? parameter)
        {
            if (parameter is EditCommandEventArgs<string> args &&
                args.Parameter is ProjectLanguage language)
            {
                language.Name = GetNotNull(args.NewValue, DefaultName);
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
