using DialogMaker.Core.Editor;
using MessagePack;

namespace DialogMaker.Core
{
    public static class SystemExtensions
    {
        public static void Save<T>(this T obj, string filePath)
        {
            var data = MessagePackSerializer.Serialize(obj);
            File.WriteAllBytes(filePath, data);
        }

        extension(Task task)
        {
            public static async Task DelaySafe(int milliseconds, CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Delay(milliseconds, cancellationToken);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
            public static async Task RunSafe(Action action, CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Run(action, cancellationToken);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
            public static async Task RunSafe(Func<Task> action, CancellationToken cancellationToken)
            {
                try
                {
                    await Task.Run(action, cancellationToken);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
        }
    }
}
