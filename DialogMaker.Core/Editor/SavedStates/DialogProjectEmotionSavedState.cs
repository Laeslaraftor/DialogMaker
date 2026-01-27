using static DialogMaker.Core.Editor.DialogProjectEmotion;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectEmotionSavedState : DialogProjectResourceObjectSavedState
    {
        public EyeInfo? LeftEye { get; set; }
        public EyeInfo? RightEye { get; set; }
        public MouthInfo? Mouth { get; set; }
    }
}
