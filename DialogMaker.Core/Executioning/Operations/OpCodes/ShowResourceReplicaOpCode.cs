using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public class ShowResourceReplicaOpCode() : OpCode(DialogByteCode.ShowResourceReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = ShowReplicaOpCode.GetCharacter(context, args[0]);
            var listener = ShowReplicaOpCode.GetCharacter(context, args[1]);
            var replica = GetString(context, args[2]);

            await DispatchHandler(context, h => h.ShowReplica(character, listener, replica, context));
        }

        #endregion

        #region Статика

        public static readonly ShowReplicaOpCode Instance = new();

        internal static IResourceString GetString(DialogExecutionContext context, int index)
        {
            var item = context.Resources.GetResource(index);

            if (item is not IResourceString str)
            {
                throw new DialogExecutionException($"Не удалось получить строку для отображения реплики. Получен тип {item?.GetType().FullName}, который не реализует {nameof(IResourceString)}");
            }

            return str;
        }

        #endregion
    }
}
