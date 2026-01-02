using System;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class WaitOpCode() : OpCode(DialogByteCode.Wait)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var time = context.Resources.GetVariable(args[0]).ToNumber();

            if (time > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(time));
            }
        }

        #endregion

        #region Статика

        public static readonly WaitOpCode Instance = new();

        #endregion
    }
}
