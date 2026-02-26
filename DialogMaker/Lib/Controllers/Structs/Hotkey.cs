using DialogMaker.Core.Editor;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class Hotkey(string name, IList<Key> keys) : ICommand
    {
        public event EventHandler<ItemEventArgs<object?>>? Pressed;
        public event EventHandler? CanExecuteChanged;

        public string Name { get; } = name;
        public ReadOnlyCollection<Key> Keys { get; } = new(keys);

        #region Управление

        public bool IsPressed(IEnumerable<Key> keys)
        {
            int keysPressed = 0;
            int totalKeys = 0;
            int keysCount = Keys.Count;

            foreach (var key in keys)
            {
                if (totalKeys >= keysCount)
                {
                    break;
                }
                if (Keys.Contains(key))
                {
                    keysPressed++;
                }

                totalKeys++;
            }

            return keysPressed == keysCount && totalKeys == keysCount;
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            Pressed?.Invoke(this, new(parameter));
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Статика

        public static readonly Hotkey Save = new("Ctrl+S", [Key.LeftCtrl, Key.S]);
        public static readonly Hotkey Copy = new("Ctrl+C", [Key.LeftCtrl, Key.C]);
        public static readonly Hotkey Cut = new("Ctrl+X", [Key.LeftCtrl, Key.X]);
        public static readonly Hotkey Paste = new("Ctrl+V", [Key.LeftCtrl, Key.V]);
        public static readonly Hotkey Delete = new(Key.Delete.ToString(), [Key.Delete]);
        public static readonly Hotkey Add = new("Shift+A", [Key.LeftShift, Key.A]);
        public static readonly Hotkey SelectAll = new("Ctrl+A", [Key.LeftCtrl, Key.A]);
        public static readonly Hotkey CancelAction = new("Ctrl+Z", [Key.LeftCtrl, Key.Z]);
        public static readonly Hotkey ApplyCancelledAction = new("Ctrl+Shift+Z", [Key.LeftCtrl, Key.LeftShift, Key.Z]);

        public static ReadOnlyCollection<Hotkey> List
        {
            get
            {
                if (field == null)
                {
                    List<Hotkey> hotkeys = [];
                    var type = typeof(Hotkey);
                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

                    foreach (var property in fields)
                    {
                        if (property.FieldType == type)
                        {
                            hotkeys.Add((Hotkey)property.GetValue(null)!);
                        }
                    }

                    field = new(hotkeys);
                }

                return field;
            }
        }

        #endregion
    }
}
