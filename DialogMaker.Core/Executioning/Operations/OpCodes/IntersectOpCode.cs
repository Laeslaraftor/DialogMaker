using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class IntersectOpCode() : OpCode(DialogByteCode.Intersect)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var info = context.Resources.GetResource(args[0]) as IJoinOperationInfo;

            if (info == null)
            {
                throw new DialogExecutionException($"Не удалось получить информацию для объединения потоков");
            }
            if (info.Outputs.Count == 0)
            {
                return;
            }

            var controller = context.ThreadManager.GetIntersectController(context, info);
            bool continueExecuting = await controller.Join(context);

            JoinOpCode.StopOrContinue(context, info, continueExecuting);
        }

        #endregion
    }
}
