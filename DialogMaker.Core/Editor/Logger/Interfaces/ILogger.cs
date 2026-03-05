using DialogMaker.Core.Editor.Messages;

namespace DialogMaker.Core.Editor
{
    public interface ILogger
    {
        public void Log(object message);
        public void Log(Message message);
    }
}
