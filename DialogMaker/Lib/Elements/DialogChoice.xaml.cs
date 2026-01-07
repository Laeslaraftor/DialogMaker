using DialogMaker.Core.Common;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogChoice : UserControl
    {
        public DialogChoice()
        {
            InitializeComponent();
        }

        public event EventHandler<ValueChangedEventArgs<int>>? ChoiceChanged;

        public ICharacter? Character
        {
            get => GetValue(CharacterProperty) as ICharacter;
            set => SetValue(CharacterProperty, value);
        }
        public IEnumerable? Variants
        {
            get => GetValue(VariantsProperty) as IEnumerable;
            set => SetValue(VariantsProperty, value);
        }
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        #region События

        private void OnVariantsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedIndex = _variants.SelectedIndex;

            int oldIndex = -1;
            var itemsSource = _variants.ItemsSource;

            if (itemsSource != null && e.RemovedItems.Count > 0)
            {
                foreach (var item in e.RemovedItems)
                {
                    int index = 0;

                    foreach (var sourceItem in itemsSource)
                    {
                        if (item.Equals(sourceItem))
                        {
                            oldIndex = index;
                            break;
                        }
                    }

                    if (oldIndex != -1)
                    {
                        break;
                    }
                }
            }

            ChoiceChanged?.Invoke(this, new(oldIndex, _variants.SelectedIndex));
        }

        private static void OnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogChoice view)
            {
                view._character.Text = (e.NewValue as ICharacter)?.Name;
            }
        }
        private static void OnVariantsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogChoice view)
            {
                view._variants.ItemsSource = e.NewValue as IEnumerable;
            }
        }
        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogChoice view)
            {
                view._variants.SelectedIndex = (int)e.NewValue;
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty CharacterProperty = DependencyProperty.Register(nameof(Character), typeof(ICharacter),
            typeof(DialogChoice), new(OnCharacterChanged));
        public static readonly DependencyProperty VariantsProperty = DependencyProperty.Register(nameof(Variants), typeof(IEnumerable),
            typeof(DialogChoice), new(OnVariantsChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
            typeof(DialogChoice), new(-1, OnSelectedIndexChanged));

        #endregion
    }
}
