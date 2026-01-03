using DialogMaker.Core.Common;
using System.Drawing;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowResourceColorReplicaOpCode() : OpCode(DialogByteCode.ShowResourceColorReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 4);

            var character = context.Resources.GetResource(args[0]) as ICharacter;
            var replica = ShowResourceReplicaOpCode.GetString(context, args[1]);
            var backgroundColorVariable = context.Resources.GetVariable(args[2]);
            var textColorVariable = context.Resources.GetVariable(args[3]);

            Color backgroundColor = ToColor(backgroundColorVariable, Color.Black);
            Color textColor = ToColor(textColorVariable, Color.White);

            await context.Handler.ShowColorReplica(character, backgroundColor, textColor, replica, context.CancellationToken);
        }

        #endregion

        #region Статика

        public static readonly ShowColorReplicaOpCode Instance = new();

        private static Color ToColor(OperandValue value, Color falloff)
        {
            if (value.Type == DialogVariableType.Number &&
                value.Value is float numberValue)
            {
                return Color.FromArgb((int)numberValue);
            }

            return falloff;
        }

        #endregion
    }
}
