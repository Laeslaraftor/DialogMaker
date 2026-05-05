namespace DialogMaker.Core
{
    internal static class CheckHelper
    {
        public static void CheckIsDisposed(Disposable disposable)
        {
            if (disposable.IsDisposed)
            {
                throw new InvalidOperationException("Невозможно сохранить очищенный объект!");
            }
        }
    }
}
