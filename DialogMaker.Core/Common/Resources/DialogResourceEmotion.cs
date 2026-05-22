using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Ресурс эмоции
    /// </summary>
    public class DialogResourceEmotion : DialogResourceObject, IEmotion
    {
        /// <summary>
        /// Создать новый экземпляр ресурса эмоции
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="resourceObject">Эмоция на основе которой будет создан ресурс</param>
        public DialogResourceEmotion(DialogResources resources, DialogProjectEmotion resourceObject)
            : base(resources, resourceObject)
        {
            LeftEye = new(resourceObject.LeftEye);
            RightEye = new(resourceObject.RightEye);
            Mouth = new(resourceObject.Mouth);
        }
        /// <summary>
        /// Создать новый экземпляр ресурса эмоции
        /// </summary>
        /// <param name="resources">Контейнер ресурсов, который будет содержать этот ресурс</param>
        /// <param name="savedState">Сохранённое состояние эмоции</param>
        public DialogResourceEmotion(DialogResources resources, DialogResourceEmotionSavedState savedState)
            : base(resources, savedState)
        {
            LeftEye = savedState.LeftEye == null ? new() : new(savedState.LeftEye);
            RightEye = savedState.RightEye == null ? new() : new(savedState.RightEye);
            Mouth = savedState.Mouth == null ? new() : new(savedState.Mouth);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override DialogResourceType ResourceType => DialogResourceType.Emotion;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public EyeInfo LeftEye { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public EyeInfo RightEye { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MouthInfo Mouth { get; }

        IEmotion.IEye IEmotion.LeftEye => LeftEye;
        IEmotion.IEye IEmotion.RightEye => RightEye;
        IEmotion.IMouth IEmotion.Mouth => Mouth;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override DialogResourceObjectSavedState CreateSavedState()
        {
            return new DialogResourceEmotionSavedState()
            {
                LeftEye = new(LeftEye),
                RightEye = new(RightEye),
                Mouth = new(Mouth),
            };
        }

        #endregion

        #region Классы

        /// <summary>
        /// Информация о брови
        /// </summary>
        /// <param name="yPosition">Положение по оси Y</param>
        /// <param name="zRotation">Наклон по оси Z</param>
        public readonly struct EyebrowInfo(float yPosition, float zRotation) : IEmotion.IEyebrow
        {
            /// <summary>
            /// Создать информацию о брови
            /// </summary>
            public EyebrowInfo(IEmotion.IEyebrow eyebrow)
                : this(eyebrow.YPosition, eyebrow.ZRotation)
            {
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float YPosition { get; } = yPosition;
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float ZRotation { get; } = zRotation;
        }
        /// <summary>
        /// Информация о глазе
        /// </summary>
        /// <param name="eyebrow">Информация о брови</param>
        /// <param name="closePercent">Степень закрытия глаза</param>
        public readonly struct EyeInfo(EyebrowInfo eyebrow, float closePercent) : IEmotion.IEye
        {
            /// <summary>
            /// Создать информацию о глазе
            /// </summary>
            public EyeInfo(IEmotion.IEye eye)
                : this(new(eye.Eyebrow), eye.ClosePercent)
            {
            }

            /// <summary>
            /// Информация о брови
            /// </summary>
            public EyebrowInfo Eyebrow { get; } = eyebrow;
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float ClosePercent { get; } = closePercent;

            IEmotion.IEyebrow IEmotion.IEye.Eyebrow => Eyebrow;
        }
        /// <summary>
        /// Информация о рте
        /// </summary>
        /// <param name="openPercent">Степень открытия рта</param>
        /// <param name="horizontalStretch">Степень сжатия по горизонтали</param>
        /// <param name="leftCornerPosition">Положение левого уголка рта по оси Y</param>
        /// <param name="rightCornerPosition">Положение правого уголка рта по оси Y</param>
        public readonly struct MouthInfo(float openPercent, float horizontalStretch, float leftCornerPosition, float rightCornerPosition)
            : IEmotion.IMouth
        {
            /// <summary>
            /// Создать информацию о рте
            /// </summary>
            public MouthInfo(IEmotion.IMouth mouth)
                : this(mouth.OpenPercent, mouth.HorizontalStretchPercent, mouth.LeftCornerYPosition, mouth.RightCornerYPosition)
            {
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float OpenPercent { get; } = openPercent;
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float HorizontalStretchPercent { get; } = horizontalStretch;
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float LeftCornerYPosition { get; } = leftCornerPosition;
            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public float RightCornerYPosition { get; } = rightCornerPosition;
        }

        #endregion
    }
}
