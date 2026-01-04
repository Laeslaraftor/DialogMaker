using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class EmptyOpCode() : OpCode(DialogByteCode.Empty)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
        }

        #endregion

        #region Статика

        public static readonly EmptyOpCode Instance = new();

        #endregion
    }
}
