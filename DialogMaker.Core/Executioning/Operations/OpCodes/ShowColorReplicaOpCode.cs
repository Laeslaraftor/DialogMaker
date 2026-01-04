using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System.Drawing;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowColorReplicaOpCode() : OpCode(DialogByteCode.ShowColorReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 4);

            var character = ShowReplicaOpCode.GetCharacter(context, args[0]);
            var replica = context.Resources.GetVariable(args[1]).ToString();
            var backgroundColorVariable = context.Resources.GetVariable(args[2]);
            var textColorVariable = context.Resources.GetVariable(args[3]);

            Color backgroundColor = ToColor(backgroundColorVariable, Color.Black);
            Color textColor = ToColor(textColorVariable, Color.White);
            ResourceString text = new(args[1], replica);

            await context.Handler.ShowColorReplica(character, backgroundColor, textColor, text, context.CancellationToken);
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