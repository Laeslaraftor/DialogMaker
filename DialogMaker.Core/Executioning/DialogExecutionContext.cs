using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogExecutionContext
    {
        public IDialogExecutionThread CurrentThread { get; }
        public IDialogExecutionResources Resources { get; }
        public IDialogExecutingHandler Handler { get; }
    }
}
