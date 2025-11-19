namespace DialogMaker.Core
{
    public static class FileExtensions
    {
        public static string GetFileName(this string filePath, bool removeExtension = true)
        {
            string lastPart = GetLastPathPart(filePath);

            if (removeExtension)
            {
                return lastPart.Split('.')[0];
            }

            return lastPart;
        }
        public static string GetFileExtension(this string filePath)
        {
            string lastPart = GetLastPathPart(filePath);
            return lastPart.Split('.')[^1];
        }

        private static string GetLastPathPart(string path)
        {
            path = path.Replace(@"\", "/");
            return path.Split('/')[^1];
        }
    }
}
