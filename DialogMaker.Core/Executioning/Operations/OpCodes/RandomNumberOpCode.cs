using Acly;
using System;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class RandomNumberOpCode() : OpCode(DialogByteCode.RandomNumber)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 4);

            var minValue = context.Resources.GetVariable(args[0]).ToNumber();
            var maxValue = context.Resources.GetVariable(args[1]).ToNumber();
            var isInt = context.Resources.GetVariable(args[2]) == true;

            float result;

            if (isInt)
            {
                result = RandomInstance.Next((int)minValue, (int)maxValue + 1);
            }
            else
            {
                result = (float)Helper.LerpUnclamped(minValue, maxValue, RandomInstance.NextDouble());
            }

            context.Resources.SetValue(args[3], result);
        }

        #endregion

        #region Статика

        public static readonly RandomNumberOpCode Instance = new();
        public static readonly Random RandomInstance = new();

        #endregion
    }
}
