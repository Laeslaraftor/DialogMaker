using DialogMaker.Core.Common;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class EmotionOpCode() : OpCode(DialogByteCode.Emotion)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);

            var character = ShowReplicaOpCode.GetCharacter(context, args[0]);
            IEmotion? emotion = null;

            if (args[1] != -1 && context.Resources.GetResource(args[1]) is IEmotion resourceEmotion)
            {
                emotion = resourceEmotion;
            }

            await context.Handler.ShowEmotion(character, emotion, context.CancellationToken);
        }

        #endregion

        #region Статика

        public static readonly EmotionOpCode Instance = new();

        #endregion
    }
}
