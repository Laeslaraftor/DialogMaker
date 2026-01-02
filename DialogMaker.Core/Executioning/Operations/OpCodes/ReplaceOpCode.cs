using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ReplaceOpCode() : OpCode(DialogByteCode.Replace)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var value1 = context.Resources.GetVariable(args[0]);

            if (value1.Value == null || 
                value1.Type != DialogVariableType.String)
            {
                return;
            }

            var searchValue = context.Resources.GetVariable(args[1]).ToString();
            var newValue = context.Resources.GetVariable(args[2]).ToString();

            string newStringValue = value1.ToString().Replace(searchValue, newValue);
            context.Resources.SetVariable(args[0], newStringValue);
        }

        #endregion
		
		#region Статика
		
		public static readonly ReplaceOpCode Instance = new();
		
		#endregion
    }
}