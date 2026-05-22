using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning.Internal;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Ресурс персонажа
    /// </summary>
    public class DialogResourceCharacter : DialogResourceObject, ICharacter
    {
        /// <summary>
        /// Создать новый экземпляр ресурса персонажа
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="character">Персонаж на основе которого будет создан ресурс</param>
        public DialogResourceCharacter(DialogResources resources, DialogProjectCharacter character) 
            : base(resources, character)
        {
            if (character.Name != null)
            {
                var item = character.Name.Resolve();
                _namePath = new(character.Name.ResourcesPath, item.Id);
            }
        }
        /// <summary>
        /// Создать новый экземпляр ресурса персонажа
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="savedState">Сохранённое состояние ресурса персонажа</param>
        public DialogResourceCharacter(DialogResources resources, DialogResourceCharacterSavedState savedState) 
            : base(resources, savedState)
        {
            if (savedState.Name != null &&
                ResourcePath.TryParse(savedState.Name, out var namePath))
            {
                _namePath = namePath;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override DialogResourceType ResourceType => DialogResourceType.Character;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Name => NameString?.Value ?? string.Empty;
        /// <summary>
        /// Строковой ресурс, представляющий имя персонажа
        /// </summary>
        public DialogResourceString? NameString
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                if (_namePath.IsEmpty)
                {
                    return null;
                }

                field ??= Resources.Package.GetResource<DialogResourceString>(_namePath);

                return field;
            }
        }

        private readonly ResourcePath _namePath;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><in</returns>
        public override IVariable ToVariable()
        {
            return new LocalVariable(Name);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            DialogResourceCharacterSavedState result = new();

            if (!_namePath.IsEmpty)
            {
                result.Name = _namePath;
            }

            return result;
        }

        #endregion
    }
}
