using DialogMaker.Core.Common;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowChoiceOpCode() : OpCode(DialogByteCode.ShowChoice)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = context.Resources.GetResource(args[0]) as ICharacter;
            var variants = _emptyStrings;

            if (context.Resources.GetResource(args[1]) is IStringCollection strings)
            {
                variants = strings.Strings;
            }

            var answer = await context.Handler.ShowChoice(character, variants);

            context.Resources.SetVariable(args[2], answer);
        }

        #endregion
		
		#region Статика
		
		public static readonly ShowChoiceOpCode Instance = new();

        private static readonly ReadOnlyCollection<string> _emptyStrings = new([]);

		#endregion
    }
}