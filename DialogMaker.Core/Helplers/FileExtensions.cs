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
        public static string GetFileDirectory(this string filePath)
        {
            return filePath.Replace(filePath.GetFileName(false), string.Empty);
        }
        public static bool CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                return true;
            }

            return false;
        }

        private static string GetLastPathPart(string path)
        {
            path = path.Replace(@"\", "/");
            return path.Split('/')[^1];
        }
    }
}
