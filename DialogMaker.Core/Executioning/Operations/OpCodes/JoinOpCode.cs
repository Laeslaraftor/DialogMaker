namespace DialogMaker.Core.Executioning
{
    public class JoinOpCode() : OpCode(DialogByteCode.Join)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);


            if (context.Resources.GetResource(args[0]) is not IJoinOperationInfo info)
            {
                throw new DialogExecutionException($"Не удалось получить информацию для объединения потоков");
            }
            if (info.Outputs.Count == 0)
            {
                return;
            }

            var controller = context.ThreadManager.GetJoinController(context, info);
            bool continueExecuting = await controller.Join(context);

            StopOrContinue(context, info, continueExecuting);
        }

        #endregion

        #region Статика

        internal static void StopOrContinue(DialogExecutionContext context, IJoinOperationInfo info, bool continueExecuting)
        {
            var threadManager = context.ThreadManager;

            if (!continueExecuting)
            {
                if (!context.CancellationToken.IsCancellationRequested &&
                    info.Outputs[0].Section == context.CurrentThread.CurrentSection)
                {
                    context.CurrentThread.Stop();
                }

                return;
            }

            void Jump(DialogPosition position)
            {
                context.CurrentThread.JumpTo(position.Section);
                context.CurrentThread.Goto(position.Operation);
            }

            if (info.Outputs.Count == 1)
            {
                var position = info.Outputs[0];
                Jump(position);
                return;
            }

            int count = 0;

            foreach (var output in info.Outputs)
            {
                if (count + 1 == info.Outputs.Count)
                {
                    Jump(output);
                    return;
                }

                threadManager.StartThread(output);
                count++;
            }
        }

        #endregion
    }
}
