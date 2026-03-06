using DialogMaker.Core.Editor.Messages;

namespace DialogMaker.Core.Editor
{
    public static class Logger
    {
        public static ILogger CurrentLogger
        {
            get => EditorSettings.Logger;
            set => EditorSettings.Logger = value;
        }

        public static void Log(object message)
        {
            CurrentLogger?.Log(message);
        }
        public static void Log(Message message)
        {
            CurrentLogger?.Log(message);
        }
    }
}
