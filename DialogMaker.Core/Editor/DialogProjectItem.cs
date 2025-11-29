using System;
using System.IO;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectItem : DialogProjectResourceObject, ISavable
    {
        public DialogProjectItem(DialogProjectResources resources, string filePath)
            : this(resources, GetResourceType(filePath), filePath)
        {
        }
        public DialogProjectItem(DialogProjectResources resources, DialogResourceType? type, string filePath)
            : this(resources, Guid.NewGuid(), type, filePath)
        {
        }
        public DialogProjectItem(DialogProjectResources resources, DialogProjectResourceItemSavedState savedState)
            : this(resources, Guid.Parse(savedState.ProjectId), savedState.ResourceType, Path.Combine(resources.Folder, savedState.FileName))
        {
            Id = savedState.Id;
        }
        private DialogProjectItem(DialogProjectResources resources, Guid id, DialogResourceType? type, string filePath)
            : base(resources, id)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"Файл \"{filePath}\" не найден", nameof(filePath));
            }
            if (type == null)
            {
                ThrowNotSupportedException(filePath);
            }

            filePath = filePath.Replace("/", @"\");
            string fileName = filePath.GetFileName(false);
            string expectedPath = Path.Combine(resources.Folder, fileName);

            if (fileName != expectedPath)
            {
                throw new ArgumentException($"Неверный путь к файлу", nameof(filePath));
            }

            FilePath = filePath;
            FileName = fileName;
            Type = type.GetValueOrDefault();
            _name = filePath.GetFileName();
        }

        public string FilePath { get; }
        public string FileName { get; }
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

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectResourceItemSavedState
            {
                FileName = FileName,
                Name = Name?.Trim() ?? string.Empty,
                ResourceType = Type
            };
        }

        public override string ToString()
        {
            return $"[Ресурс {Id}:{Type}] {Name}";
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
