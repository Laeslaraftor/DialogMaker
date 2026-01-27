using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;

namespace DialogMaker.Core.Common
{
    public class DialogResourceEmotion : DialogResourceObject, IEmotion
    {
        public DialogResourceEmotion(DialogResources resources, DialogProjectEmotion resourceObject) 
            : base(resources, resourceObject)
        {
            LeftEye = new(resourceObject.LeftEye);
            RightEye = new(resourceObject.RightEye);
            Mouth = new(resourceObject.Mouth);
        }
        public DialogResourceEmotion(DialogResources resources, DialogResourceEmotionSavedState savedState) 
            : base(resources, savedState)
        {
            LeftEye = savedState.LeftEye == null ? new() : new(savedState.LeftEye);
            RightEye = savedState.RightEye == null ? new() : new(savedState.RightEye);
            Mouth = savedState.Mouth == null ? new() : new(savedState.Mouth);
        }

        public override DialogResourceType ResourceType => DialogResourceType.Emotion;
        public EyeInfo LeftEye { get; }
        public EyeInfo RightEye { get; }
        public MouthInfo Mouth { get; }

        IEmotion.IEye IEmotion.LeftEye => LeftEye;
        IEmotion.IEye IEmotion.RightEye => RightEye;
        IEmotion.IMouth IEmotion.Mouth => Mouth;

        #region Управление

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

        public readonly struct EyebrowInfo(float yPosition, float zRotation) : IEmotion.IEyebrow
        {
            public EyebrowInfo(IEmotion.IEyebrow eyebrow)
                : this(eyebrow.YPosition, eyebrow.ZRotation)
            {
            }

            public float YPosition { get; } = yPosition;
            public float ZRotation { get; } = zRotation;
        }
        public readonly struct EyeInfo(EyebrowInfo eyebrow, float closePercent) : IEmotion.IEye
        {
            public EyeInfo(IEmotion.IEye eye)
                : this(new(eye.Eyebrow), eye.ClosePercent)
            {
            }

            public EyebrowInfo Eyebrow { get; } = eyebrow;
            public float ClosePercent { get; } = closePercent;

            IEmotion.IEyebrow IEmotion.IEye.Eyebrow => Eyebrow;
        }
        public readonly struct MouthInfo(float openPercent, float horizontalStretch, float leftCornerPosition, float rightCornerPosition) 
            : IEmotion.IMouth
        {
            public MouthInfo(IEmotion.IMouth mouth)
                : this(mouth.OpenPercent, mouth.HorizontalStretchPercent, mouth.LeftCornerYPosition, mouth.RightCornerYPosition)
            {
            }

            public float OpenPercent { get; } = openPercent;
            public float HorizontalStretchPercent { get; } = horizontalStretch;
            public float LeftCornerYPosition { get; } = leftCornerPosition;
            public float RightCornerYPosition { get; } = rightCornerPosition;
        }

        #endregion
    }
}
