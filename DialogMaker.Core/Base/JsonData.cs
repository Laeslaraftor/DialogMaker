using Newtonsoft.Json;
using System.IO;

namespace DialogMaker.Core
{
    public class JsonData : ISavedState
    {
        public const string FileExtension = "json";
        public const string FileFilter = $"Json files (.{FileExtension})|*.{FileExtension}";

        public void Save(string filePath)
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(filePath, json);
        }
    }
}
