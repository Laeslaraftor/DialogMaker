namespace DialogMaker.Lib.Elements
{
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
    public class ElementsPool<T>(Func<T> fabric) : ElementsPool(() => fabric())
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
    {
        public ElementsPool()
            : this(Activator.CreateInstance<T>)
        {
        }

        #region Управление

        public new T GetElement()
        {
            return (T)base.GetElement();
        }

        #endregion
    }
}
