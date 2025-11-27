using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectStringVariant : ObservableObject
    {
        public ProjectStringVariant(ProjectString projectString, DialogProjectStringVariant original)
        {
            String = projectString;
            Original = original;

            if (original.Language != null)
            {
                Language = projectString.Project.Languages.FirstOrDefault(l => l.Language == original.Language);
            }

            RemoveCommand = new RelayCommand(ExecuteRemove, CanRemove);
        }

        public ProjectString String { get; }
        public DialogProjectStringVariant Original { get; }
        public ProjectLanguage? Language
        {
            get
            {
                if (Original.Language == null)
                {
                    return null;
                }

                return String.Project.Languages.FirstOrDefault(l => l.ProjectId == Original.Language.ProjectId);
            }
            set
            {
                if (Original.Language != value?.Language)
                {
                    Original.Language = value?.Language;
                    InvokePropertyChanged(nameof(Language));
                }
            }
        }
        public int LanguageIndex
        {
            get
            {
                var lang = Language;

                if (lang == null)
                {
                    return -1;
                }

                return String.Project.Languages.IndexOf(lang);
            }
            set
            {
                if (LanguageIndex != value &&
                    value >= 0 && value < String.Project.Languages.Count)
                {
                    Original.Language = String.Project.Languages[value].Language;
                    InvokePropertyChanged(nameof(LanguageIndex));
                }
            }
        }
        public string Text
        {
            get => Original.Text;
            set
            {
                if (Original.Text != value)
                {
                    Original.Text = value;
                    InvokePropertyChanged(nameof(Text));
                }
            }
        }
        public ICommand RemoveCommand { get; }
        public ContextMenu ContextMenu
        {
            get
            {
                field ??= new StringVariantContextMenu(this);
                return field;
            }
        }

        #region Управление

        private bool CanRemove(object? parameter)
        {
            return String.Original.Variants.Contains(Original);
        }
        private void ExecuteRemove(object? parameter)
        {
            try
            {
                String.Original.Variants.Remove(Original);
            }
            catch (Exception error)
            {
                error.Alert();
            } 
        }

        #endregion
    }
}
