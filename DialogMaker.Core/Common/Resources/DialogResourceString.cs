using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace DialogMaker.Core.Common
{
    public class DialogResourceString : DialogResourceObject, IResourceString
    {
        public DialogResourceString(DialogResources resources, DialogProjectString str) : base(resources, str)
        {
            var variants = str.Variants.Select(v => new DialogResourceStringVariant(this, v)).ToList();
            Variants = new(variants);

            resources.Package.PropertyChanged += OnPackagePropertyChanged;
            OnPackagePropertyChanged(this, new(DialogPackage.CurrentLanguageProperty));
        }
        public DialogResourceString(DialogResources resources, DialogResourceStringSavedState savedState) : base(resources, savedState)
        {
            var variants = savedState.Variants.Select(v => new DialogResourceStringVariant(this, v)).ToList();
            Variants = new(variants);

            resources.Package.PropertyChanged += OnPackagePropertyChanged;
            OnPackagePropertyChanged(this, new(DialogPackage.CurrentLanguageProperty));
        }

        public override DialogResourceType ResourceType => DialogResourceType.String;
        public ReadOnlyCollection<DialogResourceStringVariant> Variants { get; }
        public DialogResourceStringVariant? CurrentVariant
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentVariant));
                    field = value;
                    Value = value?.Value ?? string.Empty;
                    InvokePropertyChanged(nameof(CurrentVariant));
                }
            }
        }
        public string Value
        {
            get => field ?? string.Empty;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Value));
                    field = value;
                    InvokePropertyChanged(nameof(Value));

                }
            }
        }
        IResourceFile? IResourceString.Voice => CurrentVariant?.Voice;
        string IResourceString.Text => Value;


        #region Управление

        public override string ToString()
        {
            return Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Resources.Package.PropertyChanged -= OnPackagePropertyChanged;
        }
        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            return new DialogResourceStringSavedState()
            {
                Variants = [.. Variants.Select(v => v.Save())]
            };
        }

        #endregion

        #region События

        private void OnPackagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != DialogPackage.CurrentLanguageProperty ||
                Variants.Count == 0)
            {
                return;
            }

            var currentLanguage = Resources.Package.CurrentLanguage;
            DialogResourceStringVariant? currentVariant = null;

            foreach (var variant in Variants)
            {
                if (variant.Language == currentLanguage)
                {
                    currentVariant = variant;
                    break;
                }
            }

            CurrentVariant = currentVariant;
        }

        #endregion
    }
}
