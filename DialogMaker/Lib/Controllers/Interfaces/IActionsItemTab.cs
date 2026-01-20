namespace DialogMaker.Lib.Controllers
{
    public interface IActionsItemTab
    {
        public IEnumerable<ActionButton>? Actions { get; }
    }
}
