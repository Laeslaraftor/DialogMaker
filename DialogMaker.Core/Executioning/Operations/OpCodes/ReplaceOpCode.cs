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

            var searchValueVariable = context.Resources.GetVariable(args[1]);
            var newValueResource = context.Resources.GetResource(args[2]);

            var searchValue = searchValueVariable.ToString();
            var newValue = newValueResource.ToString();

            string newStringValue = value1.ToString().Replace(searchValue, newValue);
            context.CurrentThread.Push(newStringValue);
        }

        #endregion

        #region Статика

        public static readonly ReplaceOpCode Instance = new();

        #endregion
    }
}