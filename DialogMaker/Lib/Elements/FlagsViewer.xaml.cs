using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class FlagsViewer : UserControl
    {
        public FlagsViewer()
        {
            InitializeComponent();
            ItemsPanelTemplate = DefaultItemsPanelTemplate;
        }

        public event EventHandler<ValueChangedEventArgs<Enum?>>? SelectedValuesChanged;

        public ItemsPanelTemplate ItemsPanelTemplate
        {
            get => (ItemsPanelTemplate)GetValue(ItemsPanelTemplateProperty);
            set => SetValue(ItemsPanelTemplateProperty, value);
        }
        public Enum? Enum
        {
            get => GetValue(EnumProperty) as Enum;
            set => SetValue(EnumProperty, value);
        }
        public Enum? ExcludeValues
        {
            get => GetValue(ExcludeValuesProperty) as Enum;
            set => SetValue(ExcludeValuesProperty, value);
        }
        public Enum? SelectedValues
        {
            get => GetValue(SelectedValuesProperty) as Enum;
            set => SetValue(SelectedValuesProperty, value);
        }
        public Thickness Spacing
        {
            get => (Thickness)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        private Panel? Panel => Content as Panel;

        private readonly ElementsPool<ChipView> _chips = new();
        private readonly Dictionary<Enum, string> _values = [];
        private readonly Dictionary<ChipView, Enum> _cards = [];

        #region Управление

        private void SetEnum(Enum? oldValue, Enum? newValue)
        {
            Clear();

            var panel = Panel;

            if (newValue == null || panel == null)
            {
                return;
            }

            var newValueType = newValue.GetType();
            var spacing = Spacing;

            if (SelectedValues?.GetType() != newValueType)
            {
                SelectedValues = null;
            }

            foreach (Enum value in Enum.GetValues(newValueType))
            {
                var name = value.GetEnumAttribute<NameAttribute>()?.Name;
                name ??= value.ToString();

                var card = GetCard();
                card.Margin = spacing;
                card.Text = name;

                _values.Add(value, name);
                _cards.Add(card, value);
                panel.Children.Add(card);
            }

            UpdateExclude();
            UpdateSelectedValues();
        }
        private void UpdateExclude()
        {
            var exclude = ExcludeValues;
            Func<Enum, bool> isSetted = v => false;

            if (exclude != null && exclude?.GetType() == Enum?.GetType())
            {
                isSetted = IsFlagSetted(exclude);
            }

            foreach (var info in _cards)
            {
                info.Key.IsEnabled = !isSetted(info.Value);
            }
        }
        private void UpdateSelectedValues()
        {
            var selected = SelectedValues;
            Func<Enum, bool> isSetted = v => false;

            if (selected != null && selected?.GetType() == Enum?.GetType())
            {
                isSetted = IsFlagSetted(selected);
            }

            foreach (var info in _cards)
            {
                info.Key.IsSelected = isSetted(info.Value);
            }
        }

        private void Clear()
        {
            foreach (var card in _cards.Keys)
            {
                FreeCard(card);
            }

            Panel?.Children.Clear();
            _cards.Clear();
            _values.Clear();
            _chips.Clear();
        }

        private ChipView GetCard()
        {
            var view = _chips.GetElement();
            view.RemoveFromParent();

            view.Click -= OnCardClick;
            view.Click += OnCardClick;

            return view;
        }
        private bool FreeCard(ChipView view)
        {
            if (_chips.Free(view))
            {
                view.Click -= OnCardClick;
                view.RemoveFromParent();

                return true;
            }

            return false;
        }

        #endregion

        #region События

        private void OnCardClick(object? sender, RoutedEventArgs e)
        {
            var enumValue = Enum;

            if (enumValue == null ||
                sender is not ChipView view ||
                !_cards.TryGetValue(view, out var value))
            {
                return;
            }

            var enumSelectedValues = SelectedValues;
            long selectedValues = 0;
            long currentValue = Convert.ToInt64(value);

            if (enumSelectedValues != null)
            {
                selectedValues = Convert.ToInt64(enumSelectedValues);
            }
            if (view.IsSelected)
            {
                selectedValues &= ~currentValue;
            }
            else
            {
                selectedValues |= currentValue;
            }

            SelectedValues = (Enum)Enum.ToObject(enumValue.GetType(), selectedValues);
        }

        private static void OnItemsPanelTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FlagsViewer view)
            {
                return;
            }

            view.Clear();
            var enumType = view.Enum;
            var panel = (e.NewValue as ItemsPanelTemplate)?.LoadContent() as Panel;
            panel?.IsItemsHost = false;

            view.Content = panel;
            view.SetEnum(enumType, enumType);
        }
        private static void OnEnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlagsViewer view)
            {
                view.SetEnum(e.OldValue as Enum, e.NewValue as Enum);
            }
        }
        private static void OnExcludeValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlagsViewer view)
            {
                view.UpdateExclude();
            }
        }
        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlagsViewer view)
            {
                view.UpdateSelectedValues();
                view.SelectedValuesChanged?.Invoke(d, new(e.OldValue as Enum, e.NewValue as Enum));
            }
        }
        private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FlagsViewer view ||
                e.NewValue is not Thickness spacing)
            {
                return;
            }

            foreach (var card in view._cards.Keys)
            {
                card.Margin = spacing;
            }
        }

        #endregion

        #region Depdendency

        public static readonly DependencyProperty ItemsPanelTemplateProperty = DependencyProperty.Register(nameof(ItemsPanelTemplate), typeof(ItemsPanelTemplate),
            typeof(FlagsViewer), new(OnItemsPanelTemplateChanged));
        public static readonly DependencyProperty EnumProperty = DependencyProperty.Register(nameof(Enum), typeof(Enum),
            typeof(FlagsViewer), new(OnEnumChanged));
        public static readonly DependencyProperty ExcludeValuesProperty = DependencyProperty.Register(nameof(ExcludeValues), typeof(Enum),
            typeof(FlagsViewer), new(OnExcludeValuesChanged));
        public static readonly DependencyProperty SelectedValuesProperty = DependencyProperty.Register(nameof(SelectedValues), typeof(Enum),
            typeof(FlagsViewer), new(OnSelectedValuesChanged));
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing), typeof(Thickness),
            typeof(FlagsViewer), new(OnSpacingChanged));

        private static ItemsPanelTemplate DefaultItemsPanelTemplate
        {
            get
            {
                if (field == null)
                {
                    field = new()
                    {
                        VisualTree = new(typeof(StackPanel))
                    };
                    field.Seal();
                }

                return field;
            }
        }

        #endregion

        #region Статика

        private static Func<Enum, bool> IsFlagSetted(Enum? flags)
        {
            if (flags == null)
            {
                return v => false;
            }

            var enumValues = flags.GetValues().ToArray();

            return value =>
            {
                return enumValues.Contains(value);
            };
        }

        #endregion
    }
}
