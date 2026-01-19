namespace DialogMaker.Lib
{
    public interface IFillerValidator<T>
    {
        public bool Validate(T item);
    }
}
