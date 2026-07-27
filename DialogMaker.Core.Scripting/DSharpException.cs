namespace DialogMaker.Core.Scripting
{
    public class DSharpException : Exception
    {
        public DSharpException()
        {
        }
        public DSharpException(string message) : base(message)
        {
        }
        public DSharpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
