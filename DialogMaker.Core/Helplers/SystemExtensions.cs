using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core
{
    public static class SystemExtensions
    {
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
