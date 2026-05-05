using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class CharacterView : UserControl
    {
        public CharacterView()
        {
            InitializeComponent();

            _idBlock.EditCommand = ProjectResourceItem.IdEditCommand;
        }

        public ProjectCharacter? Character
        {
            get => GetValue(CharacterProperty) as ProjectCharacter;
            set => SetValue(CharacterProperty, value);
        }

        #region События

        private void OnCharacterChanged(ProjectCharacter? oldValue, ProjectCharacter? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            _idBlock.DataContext = newValue;
            _idBlock.EditCommandParameter = newValue;
            _nameReference.Item = newValue?.Name?.Item;
            _flagsView.Value = newValue?.Original.Resources.Flags;
            ContextMenu = newValue?.ContextMenu;

            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnCharacterPropertyChanged;
            }
            if (newValue != null)
            {
                newValue.PropertyChanged += OnCharacterPropertyChanged;
            }
        }

        private void OnNameReferenceItemChanged(object sender, ValueChangedEventArgs<ProjectResourceItem> e)
        {
            var character = Character;

            if (character == null)
            {
                return;
            }

            ProjectReference<ProjectString, DialogProjectString>? value = null;

            if (e.NewValue != null)
            {
                value = (ProjectString)e.NewValue;
            }

            character.Name = value;
        }
        private void OnCharacterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Item")
            {
                return;
            }

            _nameReference.Item = Character?.Name?.Item;
        }

        private static void OnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CharacterView view)
            {
                view.OnCharacterChanged(e.OldValue as ProjectCharacter, e.NewValue as ProjectCharacter);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty CharacterProperty = DependencyProperty.Register(nameof(Character), typeof(ProjectCharacter),
            typeof(CharacterView), new(OnCharacterChanged));

        #endregion
    }
}
