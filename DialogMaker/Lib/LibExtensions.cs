using DialogMaker.Lib;

namespace DialogMaker
{
    public static class LibExtensions
    {
        public static void Alert(this Exception error)
        {
            Alerts.Show(error);
        }
        public static bool Try(Action method)
        {
            try
            {
                method();
            }
            catch (Exception error)
            {
                error.Alert();
                return false;
            }

            return true;
        }
    }
}
