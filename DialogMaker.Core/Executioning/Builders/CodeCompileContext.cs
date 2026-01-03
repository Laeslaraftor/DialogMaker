using System.IO;

namespace DialogMaker.Core.Executioning.Builders
{
    internal readonly struct CodeCompileContext(Stream codeStream, DialogExecutionContextBuilder context)
    {
        public Stream CodeStream { get; } = codeStream;
        public DialogExecutionContextBuilder ContextBuilder { get; } = context;
    }
}
