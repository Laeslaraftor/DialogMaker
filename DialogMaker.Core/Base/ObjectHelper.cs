namespace DialogMaker.Core
{
    public static class ObjectHelper
    {
        public static void DisposeAll(IEnumerable<IDisposable> disposables)
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
