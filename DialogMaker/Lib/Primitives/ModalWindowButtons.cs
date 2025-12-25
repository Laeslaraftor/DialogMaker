namespace DialogMaker.Lib
{
    [Flags]
    public enum ModalWindowButtons
    {
        Main = 1 << 0,
        Secondary = 1 << 1,
        All = Main | Secondary
    }
}
