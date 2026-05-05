namespace DialogMaker.Core.Executioning
{
    public class SetOpCode() : OpCode(DialogByteCode.Set)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);

            var value2 = context.Resources.GetResource(args[1]);
            context.Resources.SetValue(args[0], value2);
        }

        #endregion

        #region Статика

        public static readonly SetOpCode Instance = new();

        #endregion
    }
}
