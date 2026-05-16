using Newtonsoft.Json;

namespace DialogMaker.Core
{
    internal static class SavedState
    {
        public static T Restore<T, TSavedState>(string filePath) where TSavedState : JsonData
        {
            return Restore<T, TSavedState>(filePath, savedState =>
            {
                return (T)Activator.CreateInstance(typeof(T), filePath.GetFileDirectory(), savedState);
            });
        }
        public static T Restore<T, TSavedState>(string filePath, Func<TSavedState, T> fabric) where TSavedState : JsonData
        {
            var savedState = Restore<TSavedState>(filePath);
            return fabric(savedState);
        }
        public static TSavedState Restore<TSavedState>(string filePath) where TSavedState : JsonData
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"Файл \"{filePath}\" не найден", nameof(filePath));
            }

            string json = File.ReadAllText(filePath);
            var savedState = JsonConvert.DeserializeObject<TSavedState>(json);

            return savedState ?? throw new InvalidDataException("Не удалось прочитать файл");
        }
    }
}
