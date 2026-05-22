namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс эмоции
    /// </summary>
    public interface IEmotion : IResourceItem
    {
        /// <summary>
        /// Левый глаз
        /// </summary>
        public IEye LeftEye { get; }
        /// <summary>
        /// Правый глаз
        /// </summary>
        public IEye RightEye { get; }
        /// <summary>
        /// Рот
        /// </summary>
        public IMouth Mouth { get; }

        #region Интерфейсы

        /// <summary>
        /// Интерфейс настроек брови
        /// </summary>
        public interface IEyebrow
        {
            /// <summary>
            /// Позиция по оси Y
            /// </summary>
            public float YPosition { get; }
            /// <summary>
            /// Наклон по оси Z
            /// </summary>
            public float ZRotation { get; }
        }
        /// <summary>
        /// Интерфейс настроек глаза
        /// </summary>
        public interface IEye
        {
            /// <summary>
            /// Бровь
            /// </summary>
            public IEyebrow Eyebrow { get; }
            /// <summary>
            /// Степень закрытия глаза
            /// </summary>
            public float ClosePercent { get; }
        }
        /// <summary>
        /// Интерфейс настроек рта
        /// </summary>
        public interface IMouth
        {
            /// <summary>
            /// Степень открытости
            /// </summary>
            public float OpenPercent { get; }
            /// <summary>
            /// Степень сжатия по горизонтали
            /// </summary>
            public float HorizontalStretchPercent { get; }
            /// <summary>
            /// Позиция левого уголка рта по оси Y
            /// </summary>
            public float LeftCornerYPosition { get; }
            /// <summary>
            /// Позиция правого уголка рта по оси Y
            /// </summary>
            public float RightCornerYPosition { get; }
        }

        #endregion
    }
}
