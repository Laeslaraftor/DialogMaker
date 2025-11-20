using DialogMaker.Core.Editor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DialogMaker.Core
{
    public static class SavedState
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

            if (savedState == null)
            {
                throw new InvalidDataException("Не удалось прочитать файл");
            }

            return savedState;
        }

        public static void Save<TSavedState, TOriginal>(List<TSavedState> buffer, IEnumerable<TOriginal> items) where TSavedState : ISavedState where TOriginal : ISavable
        {
            foreach (var item in items)
            {
                buffer.Add((TSavedState)item.Save());
            }
            ;
        }
    }
}
