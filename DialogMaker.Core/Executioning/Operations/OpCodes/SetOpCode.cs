using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class SetOpCode() : OpCode(DialogByteCode.Set)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);

            var value2 = context.Resources.GetVariable(args[1]);
            context.Resources.SetVariable(args[0], value2);
        }

        #endregion

        #region Статика

        public static readonly SetOpCode Instance = new();

        #endregion
    }
}
