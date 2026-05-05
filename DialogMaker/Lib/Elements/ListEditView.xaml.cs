using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using DialogMaker.Lib.Converters;
using DialogMaker.Lib.InputFields;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class ListEditView : UserControl
    {
        public ListEditView()
        {
            InitializeComponent();

            _converter.EditorChanged += OnConverterEditorChanged;
            _itemsView.ItemsSource = _inputs;
        }

        public IEditableList? EditableList
        {
            get => GetValue(EditableListProperty) as IEditableList;
            set => SetValue(EditableListProperty, value);
        }
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public Action<InputField>? InputFieldHandler
        {
            get => GetValue(InputFieldHandlerProperty) as Action<InputField>;
            set => SetValue(InputFieldHandlerProperty, value);
        }

        private int Count
        {
            get
            {
                var list = EditableList;

                if (list == null)
                {
                    return 0;
                }

                int count = 0;

                foreach (var _ in list)
                {
                    count++;
                }

                return count;
            }
        }

        private readonly ObjectEditorsConverter _converter = new();
        private readonly ObservableCollection<InputField> _inputs = [];
        private CollectionSynchronizer<object?, InputField>? _collectionsSync;
        private bool _disableSync;

        #region Управление

        private void SetEditableList(IEditableList? oldValue, IEditableList? newValue)
        {
            _collectionsSync?.Dispose();
            _collectionsSync = null;

            _inputs.Clear();
            _converter.Clear();

            UpdateAddButton();
            UpdateRemoveButton();

            if (newValue is not INotifyCollectionChanged notify)
            {
                return;
            }

            _collectionsSync = new(notify, _inputs, _converter);
        }
        private void UpdateRemoveButton()
        {
            var list = EditableList;

            if (list == null)
            {
                _removeButton.IsEnabled = false;
                return;
            }

            _removeButton.IsEnabled = Count > 0 && _itemsView.SelectedIndex != -1;
        }
        private void UpdateAddButton()
        {
            var list = EditableList;
            _addButton.IsEnabled = list != null && list.CanAddNew;
        }
        private void UpdateMoveButtons()
        {
            var list = EditableList;

            if (list == null)
            {
                _moveUpButton.IsEnabled = false;
                _moveDownButton.IsEnabled = false;
                return;
            }

            int selectedIndex = _itemsView.SelectedIndex;

            _moveUpButton.IsEnabled = selectedIndex > 0;
            _moveDownButton.IsEnabled = _inputs.Count - 1 > selectedIndex && selectedIndex != -1;
        }

        private void MoveElement(int index, int offset)
        {
            var list = EditableList;
            int newIndex = index + offset;

            if (list == null || 0 > newIndex || newIndex >= _inputs.Count)
            {
                return;
            }

            var currentValue = list.GetValue(index);
            var otherValue = list.GetValue(newIndex);

            list.SetValue(otherValue, index);
            list.SetValue(currentValue, newIndex);

            _collectionsSync?.SyncFirstToSecond();

            _itemsView.SelectedIndex = newIndex;
        }

        #endregion

        #region События

        private void OnConverterEditorChanged(object? sender, CollectionItemEventArgs<InputField> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                InputFieldHandler?.Invoke(e.Item);
                e.Item.PropertyChanged += OnItemPropertyChanged;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disableSync)
            {
                return;
            }

            var list = EditableList;

            if (list == null ||
                sender is not InputField field ||
                e.PropertyName != "Value")
            {
                return;
            }

            var index = _inputs.IndexOf(field);

            if (index != -1)
            {
                object? value = field.Value;

                if (value is ProjectResourceItem item)
                {
                    value = DialogProjectReference.Create(item.Model);
                }

                try
                {
                    _disableSync = true;
                    list.SetValue(value, index);
                    _disableSync = false;
                }
                catch (Exception error)
                {
                    _disableSync = false;
                    error.Log();
                }
            }
        }

        private void OnAddButtonClicked(object sender, RoutedEventArgs e)
        {
            var list = EditableList;

            if (list != null)
            {
                list.AddNew();
                list.CommitNew();
            }
        }
        private void OnRemoveButtonClicked(object sender, RoutedEventArgs e)
        {
            int index = _itemsView.SelectedIndex;
            var list = EditableList;

            if (list == null || !list.CanRemove || index == -1)
            {
                return;
            }

            list.RemoveAt(index);
        }
        private void OnMoveUpButtonClicked(object sender, RoutedEventArgs e)
        {
            MoveElement(_itemsView.SelectedIndex, -1);
        }
        private void OnMoveDownButtonClicked(object sender, RoutedEventArgs e)
        {
            MoveElement(_itemsView.SelectedIndex, 1);
        }
        private void OnItemsViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRemoveButton();
            UpdateMoveButtons();
        }

        private static void OnEditableListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListEditView view)
            {
                return;
            }

            var newValue = e.NewValue as IEditableList;
            var newValueType = newValue?.GetType();
            Type newItemsType = typeof(object);

            if (newValueType != null && newValueType.IsGenericType)
            {
                var genericTypes = newValueType.GetGenericArguments();

                if (genericTypes.Length == 1)
                {
                    newItemsType = genericTypes[0];
                }
            }

            view._converter.ItemsType = newItemsType;
            view.SetEditableList(e.OldValue as IEditableList, newValue);
        }
        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListEditView view)
            {
                view._placeholder.Text = e.NewValue?.ToString();
            }
        }
        private static void OnInputFieldHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListEditView view ||
                e.NewValue is not Action<InputField> handler)
            {
                return;
            }

            foreach (var field in view._inputs)
            {
                handler(field);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty EditableListProperty = DependencyProperty.Register(nameof(EditableList), typeof(IEditableList),
            typeof(ListEditView), new(OnEditableListChanged));
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string),
            typeof(ListEditView), new(string.Empty, OnPlaceholderChanged));
        public static readonly DependencyProperty InputFieldHandlerProperty = DependencyProperty.Register(nameof(InputFieldHandler), typeof(Action<InputField>),
            typeof(ListEditView), new(OnInputFieldHandlerChanged));

        #endregion
    }
}
