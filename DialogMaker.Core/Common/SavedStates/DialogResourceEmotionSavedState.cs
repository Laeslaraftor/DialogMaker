using DialogMaker.Core.Editor;
using MessagePack;

namespace DialogMaker.Core.Common.SavedStates
{
    [MessagePackObject(SuppressSourceGeneration = false)]
    public class DialogResourceEmotionSavedState : DialogResourceObjectSavedState
    {
        [Key(1)]
        public DialogProjectEmotion.EyeInfo? LeftEye { get; set; }
        [Key(2)]
        public DialogProjectEmotion.EyeInfo? RightEye { get; set; }
        [Key(3)]
        public DialogProjectEmotion.MouthInfo? Mouth { get; set; }
    }
}
