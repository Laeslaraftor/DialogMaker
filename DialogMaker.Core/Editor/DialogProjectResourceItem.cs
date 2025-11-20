using System;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectResourceItem : ObservableObject, ISavable
    {
        public DialogProjectResourceItem(string filePath)
            : this(GetResourceType(filePath), filePath)
        {
        }
        public DialogProjectResourceItem(DialogResourceType? type, string filePath)
        {
            if (type == null)
            {
                ThrowNotSupportedException($"Неподдерживаемый формат файла \"{filePath}\"");
            }

            Id = Guid.NewGuid();
            FilePath = filePath;
            Type = type.GetValueOrDefault();
            _name = filePath.GetFileName();
        }
        public DialogProjectResourceItem(DialogProjectResources resources, DialogProjectResourceItemSavedState savedState)
        {
            Id = Guid.Parse(savedState.Id);
            FilePath = Path.Combine(resources.Folder, savedState.FileName);

            if (!File.Exists(FilePath))
            {
                throw new ArgumentException($"Файл {FilePath} ресурса {savedState.Name} не найден.");
            }
         
            Type = savedState.ResourceType;
            _name = savedState.Name;
        }

        public Guid Id { get; }
        public string FilePath { get; }
        public DialogResourceType Type { get; }
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }

        private string _name;

        #region Управление

        public ISavedState Save()
        {
            return new DialogProjectResourceItemSavedState
            {
                Id = Id.ToString(),
                FileName = FilePath.GetFileName(false),
                Name = Name,
                ResourceType = Type
            };
        }

        #endregion

        #region Статика

        private static readonly string[] _audioExtensions = { "ogg", "mp3" };
        private static readonly string[] _videoExtensions = { "mp4" };
        private static readonly string[] _imageExtensions = { "png", "jpg", "jpeg" };

        public static DialogResourceType? GetResourceType(string filePath)
        {
            string extension = filePath.GetFileExtension();

            if (_audioExtensions.Contains(extension))
            {
                return DialogResourceType.Audio;
            }
            else if (_imageExtensions.Contains(extension))
            {
                return DialogResourceType.Image;
            }
            else if (_videoExtensions.Contains(extension))
            {
                return DialogResourceType.Video;
            }

            return null;
        }
        public static void ThrowNotSupportedException(string filePath)
        {
            throw new NotSupportedException($"Неподдерживаемый формат файла \"{filePath}\"");
        }

        #endregion
    }
}
