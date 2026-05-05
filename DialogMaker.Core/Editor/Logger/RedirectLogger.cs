namespace DialogMaker.Core.Editor
{
    [LoggerImplementation]
    internal class RedirectLogger : Acly.ILogger
    {
        public void Message(object obj)
        {
            Logger.Log(obj);
        }
        public void Warning(object obj)
        {
            Logger.Log(obj);
        }
        public void Error(object obj)
        {
            Logger.Log(obj);
        }
    }
}
