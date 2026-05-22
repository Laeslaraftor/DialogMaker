using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Вариант строки
    /// </summary>
    public class DialogResourceStringVariant
    {
        /// <summary>
        /// Создать новый экземпляр варианта строки
        /// </summary>
        /// <param name="str">Ресурс строки, который является владельцем варианта</param>
        /// <param name="variant">Редактируемый вариант строки на основе которого будет создан новый экземпляр</param>
        public DialogResourceStringVariant(DialogResourceString str, DialogProjectStringVariant variant)
        {
            String = str;
            Value = variant.Text;

            if (variant.Language != null)
            {
                Language = str.Resources.Package.Languages[variant.Language.Id];
            }
            if (variant.Voice != null)
            {
                var item = variant.Voice.Resolve();
                _voicePath = new(variant.Voice.ResourcesPath, item.Id);
            }
        }
        /// <summary>
        /// Создать новый экземпляр варианта строки
        /// </summary>
        /// <param name="str">Ресурс строки, который является владельцем варианта</param>
        /// <param name="savedState">Сохранённое состояние варианта строки</param>
        public DialogResourceStringVariant(DialogResourceString str, DialogResourceStringVariantSavedState savedState)
        {
            String = str;
            Value = savedState.Value;

            if (savedState.Language != null)
            {
                Language = str.Resources.Package.Languages[savedState.Language];
            }
            if (savedState.Voice != null && ResourcePath.TryParse(savedState.Voice, out var voicePath))
            {
                _voicePath = voicePath;
            }
        }

        /// <summary>
        /// Ресурс строки, который является владельцем варианта
        /// </summary>
        public DialogResourceString String { get; }
        /// <summary>
        /// Язык варианта
        /// </summary>
        public DialogLanguage? Language { get; }
        /// <summary>
        /// Значение варианта
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Ресурс файла озвучки
        /// </summary>
        public DialogResourceFile? Voice
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                if (_voicePath.IsEmpty)
                {
                    return null;
                }

                field ??= String.Resources.Package.GetResource<DialogResourceFile>(_voicePath);

                return field;
            }
        }

        private readonly ResourcePath _voicePath;

        #region Управление

        /// <summary>
        /// Получить сохранённое состояние варианта строки
        /// </summary>
        /// <returns>Сохранённое состояние варианта строки</returns>
        public DialogResourceStringVariantSavedState Save()
        {
            DialogResourceStringVariantSavedState result = new()
            {
                Language = Language?.Id,
                Value = Value
            };

            if (!_voicePath.IsEmpty)
            {
                result.Voice = _voicePath;
            }

            return result;
        }

        #endregion
    }
}
