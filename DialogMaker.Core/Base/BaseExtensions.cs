namespace DialogMaker.Core
{
    public static class BaseExtensions
    {
        public static string Repeat(this string str, int count)
        {
            for (int i = 0; i < count; i++)
            {
                str += str;
            }

            return str;
        }
    }
}
