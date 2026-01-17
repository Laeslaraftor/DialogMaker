using MessagePack;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
                    Debug.WriteLine(error);
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
                    Debug.WriteLine(error);
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
                    Debug.WriteLine(error);
                }
            }
        }
    }
}
