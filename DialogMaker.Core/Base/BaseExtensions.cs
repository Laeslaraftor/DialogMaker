namespace DialogMaker.Core
{
    internal static class BaseExtensions
    {
        extension(string str)
        {
            public string Repeat(int count)
            {
                string result = string.Empty;

                for (int i = 0; i < count; i++)
                {
                    result += str;
                }

                return result;
            }
        }
    }
}
