using System;
using System.Linq;

namespace DialogMaker.Core
{
    public class DialogProjectResourceItem : ObservableObject
    {
        public DialogProjectResourceItem(string filePath)
        {
            var type = GetResourceType(filePath);

            if (type == null)
            {
                throw new ArgumentException($"Неподдерживаемый формат файла \"{filePath}\"", nameof(filePath));
            }

            Id = Guid.NewGuid();
            FilePath = filePath;
            _name = filePath.GetFileName();
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

        #endregion
    }
}
