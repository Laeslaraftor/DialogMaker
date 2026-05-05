using System.Runtime.Serialization;

namespace DialogMaker.Core.Executioning
{
    public class DialogExecutionException : Exception
    {
        public DialogExecutionException()
        {
        }
        public DialogExecutionException(string message) : base(message)
        {
        }
        public DialogExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
        protected DialogExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
