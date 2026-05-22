using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning.Internal;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Ресурс строки
    /// </summary>
    public class DialogResourceString : DialogResourceObject, IResourceString
    {
        /// <summary>
        /// Создать новый экземпляр ресурса строки
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="str">Строка на основе которой будет создать ресурс</param>
        public DialogResourceString(DialogResources resources, DialogProjectString str) 
            : base(resources, str)
        {
            var variants = str.Variants.Select(v => new DialogResourceStringVariant(this, v)).ToList();
            Variants = new(variants);

            resources.Package.PropertyChanged += OnPackagePropertyChanged;
            OnPackagePropertyChanged(this, new(DialogPackage.CurrentLanguageProperty));
        }
        /// <summary>
        /// Создать новый экземпляр ресурса строки
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="savedState">Сохранённое состояние строки</param>
        public DialogResourceString(DialogResources resources, DialogResourceStringSavedState savedState) 
            : base(resources, savedState)
        {
            var variants = savedState.Variants.Select(v => new DialogResourceStringVariant(this, v)).ToList();
            Variants = new(variants);

            resources.Package.PropertyChanged += OnPackagePropertyChanged;
            OnPackagePropertyChanged(this, new(DialogPackage.CurrentLanguageProperty));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override DialogResourceType ResourceType => DialogResourceType.String;
        /// <summary>
        /// Вариации строки
        /// </summary>
        public ReadOnlyCollection<DialogResourceStringVariant> Variants { get; }
        /// <summary>
        /// Текущая используемая вариация строки (зависит от выбранного языка в <see cref="DialogPackage.CurrentLanguage"/>)
        /// </summary>
        public DialogResourceStringVariant? CurrentVariant
        {
            get;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(CurrentVariant));
                    field = value;
                    Value = value?.Value ?? string.Empty;
                    OnPropertyChanged(nameof(CurrentVariant));
                }
            }
        }
        /// <summary>
        /// Значение строки
        /// </summary>
        public string Value
        {
            get => field ?? string.Empty;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Value));
                    field = value;
                    OnPropertyChanged(nameof(Value));

                }
            }
        }
        IResourceFile? IResourceString.Voice => CurrentVariant?.Voice;
        string IResourceString.Text => Value;


        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override IVariable ToVariable()
        {
            return new LocalVariable(Value);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="isDisposing"><inheritdoc/></param>
        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Resources.Package.PropertyChanged -= OnPackagePropertyChanged;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
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
