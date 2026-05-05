using DialogMaker.Core.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class HotkeysController : Disposable
    {
        public HotkeysController(DependencyObject owner)
        {
            Owner = owner;
            _pressedKeys.ItemChanged += OnPressedKeysItemChanged;

            Keyboard.AddPreviewKeyDownHandler(owner, OnKeyboardKeyDown);
            Keyboard.AddPreviewKeyUpHandler(owner, OnKeyboardKeyUp);
        }

        public event EventHandler<ItemEventArgs<Hotkey>>? HotkeyPressed;

        public DependencyObject Owner { get; }
        public Hotkey? CurrentHotkey
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(CurrentHotkey));
                    field = value;

                    if (value != null)
                    {
                        value.Execute(this);
                        HotkeyPressed?.Invoke(this, new(value));
                    }

                    OnPropertyChanged(nameof(CurrentHotkey));
                }
            }
        }

        private readonly EditableCollection<Key> _pressedKeys = [];

        #region События

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _pressedKeys.ItemChanged -= OnPressedKeysItemChanged;
            _pressedKeys.Clear();
            CurrentHotkey = null;

            Keyboard.RemovePreviewKeyDownHandler(Owner, OnKeyboardKeyDown);
            Keyboard.RemovePreviewKeyUpHandler(Owner, OnKeyboardKeyUp);
        }

        #endregion

        #region События

        private void OnPressedKeysItemChanged(object? sender, CollectionItemEventArgs<Key> e)
        {
            if (e.Action == CollectionItemAction.Move)
            {
                return;
            }

            foreach (var hotkey in Hotkey.List)
            {
                if (hotkey.IsPressed(_pressedKeys) && hotkey.CanExecute(this))
                {
                    CurrentHotkey = hotkey;
                    _pressedKeys.Clear();
                    return;
                }
            }

            CurrentHotkey = null;
        }

        private void OnKeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.System &&
                !IgnoreFocusedElement() &&
                !e.Handled && !_pressedKeys.Contains(e.Key))
            {
                e.Handled = true;
                _pressedKeys.Add(e.Key);
            }
        }
        private void OnKeyboardKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.System &&
                !IgnoreFocusedElement() &&
                !e.Handled && _pressedKeys.Remove(e.Key))
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Статика

        private static bool IgnoreFocusedElement()
        {
            var element = Keyboard.FocusedElement;
            return element is TextBox || element is RichTextBox;
        }

        #endregion
    }
}
