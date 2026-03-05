using DialogMaker.Core.Editor.Messages;
using System.Diagnostics;

namespace DialogMaker.Core.Editor
{
    public class ConsoleLogger : ILogger
    {
        public void Log(object message)
        {
            Debug.WriteLine(message);
        }
        public void Log(Message message)
        {
            Debug.WriteLine($"{message.Title}: {message.Text}");
        }

        #region Статика

        public static readonly ConsoleLogger Instance = new();

        #endregion
    }
}
