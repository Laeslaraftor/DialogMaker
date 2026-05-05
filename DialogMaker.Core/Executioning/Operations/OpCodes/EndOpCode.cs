namespace DialogMaker.Core.Executioning
{
    public class EndOpCode() : OpCode(DialogByteCode.End)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            context.ThreadManager.Stop();
        }

        #endregion

        #region Статика

        public static readonly EndOpCode Instance = new();

        #endregion
    }
}