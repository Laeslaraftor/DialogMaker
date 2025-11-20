using DialogMaker.Lib;

namespace DialogMaker
{
    public static class LibExtensions
    {
        public static void Alert(this Exception error)
        {
            Alerts.Show(error);
        }
    }
}
