namespace DialogMaker.Core.Common
{
    public interface IEmotion : IResourceItem
    {
        public IEye LeftEye { get; }
        public IEye RightEye { get; }
        public IMouth Mouth { get; }

        #region Интерфейсы

        public interface IEyebrow
        {
            public float YPosition { get; }
            public float ZRotation { get; }
        }
        public interface IEye
        {
            public IEyebrow Eyebrow { get; }
            public float ClosePercent { get; }
        }
        public interface IMouth
        {
            public float OpenPercent { get; }
            public float HorizontalStretchPercent { get; }
            public float LeftCornerYPosition { get; }
            public float RightCornerYPosition { get; }
        }

        #endregion
    }
}
