using DialogMaker.Core;
using DialogMaker.Lib;
using System.Collections;
using System.Windows.Input;

namespace DialogMaker.Editor.Filters
{
    public class ProjectResourcesFilter : ObservableObject, ICollectionItemsFilter
    {
        public ProjectResourcesFilter()
        {
            Flags = AllFlags;
            SearchCommand = new RelayCommand(CanSearchCommand, ExecuteSearchCommand);
        }

        public event EventHandler? FilterChanged;

        public string? SearchValue
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(SearchValue));
                    field = value;
                    OnPropertyChanged(nameof(SearchValue));
                }
            }
        }
        public DialogResourcesFlags Flags
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Flags));
                    field = value;
                    OnPropertyChanged(nameof(Flags));
                }
            }
        }
        public ICommand SearchCommand { get; }

        #region Управление

        public bool Check(IEnumerable Collection, object Item)
        {
            if (Item is not ProjectResourceItem item)
            {
                return false;
            }

            var flags = Flags;
            var searchValue = SearchValue;
            bool isNullSearchValue = string.IsNullOrEmpty(searchValue);

            if (flags == AllFlags && isNullSearchValue)
            {
                return true;
            }
            if (!Flags.HasFlag(item.Model.Resources.Flags))
            {
                return false;
            }
            if (!isNullSearchValue)
            {
                return item.ContainsValue(searchValue!);
            }

            return true;
        }

        private bool CanSearchCommand(object? parameter)
        {
            return parameter is ValueChangedEventArgs<string>;
        }
        private void ExecuteSearchCommand(object? parameter)
        {
            if (parameter is ValueChangedEventArgs<string> args)
            {
                SearchValue = args.NewValue;
            }
        }

        #endregion

        #region События

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Статика

        public static readonly ProjectResourcesFilter Instance = new();
        public static readonly DialogResourcesFlags AllFlags = DialogResourcesFlags.Pack |
                                                               DialogResourcesFlags.Root |
                                                               DialogResourcesFlags.Dialog;

        #endregion
    }
}
