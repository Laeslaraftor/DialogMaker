using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectStringVariant : Disposable
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

            original.PropertyChanging += OnOriginalPropertyChanging;
            original.PropertyChanged += OnOriginalPropertyChanged;
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
                    OnPropertyChanged(nameof(Language));
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
                    try
                    {
                        Original.Language = String.Project.Languages[value].Language;
                    }
                    catch (Exception error)
                    {
                        error.Log();
                    }

                    OnPropertyChanged(nameof(LanguageIndex));
                }
            }
        }
        public string Text
        {
            get => Original.Text;
            set => Original.Text = value;
        }
        public ProjectReference<ProjectFile, DialogProjectItem>? Voice
        {
            get
            {
                if (Original.Voice == null)
                {
                    return null;
                }
                if (_voice?.Item.Original.ProjectId != Original.Voice?.ItemId)
                {
                    _voice = Original.Voice == null ? null : new(String.Project, Original.Voice);
                }

                return _voice;
            }
            set
            {
                if (Original.Voice?.ItemId == value?.Item.Original.ProjectId)
                {
                    return;
                }
                if (value?.Item == null)
                {
                    Original.Voice = null;
                }
                else
                {
                    Original.Voice = value.Item.Original;
                }

                _voice = value;
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

        private ProjectReference<ProjectFile, DialogProjectItem>? _voice;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Original.PropertyChanging -= OnOriginalPropertyChanging;
            Original.PropertyChanged -= OnOriginalPropertyChanged;
        }

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
                error.Log();
            }
        }

        #endregion

        #region События

        private void OnOriginalPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            OnPropertyChanging(e);
        }
        private void OnOriginalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        #endregion
    }
}
