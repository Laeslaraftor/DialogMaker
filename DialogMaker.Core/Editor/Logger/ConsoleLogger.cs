using DialogMaker.Core.Editor.Messages;

namespace DialogMaker.Core.Editor
{
    public class ConsoleLogger : ILogger
    {
        public void Log(object message)
        {
            Logger.Log(message);
        }
        public void Log(Message message)
        {
            Logger.Log($"{message.Title}: {message.Text}");
        }

        #region Статика

        public static readonly ConsoleLogger Instance = new();

        #endregion
    }
}
