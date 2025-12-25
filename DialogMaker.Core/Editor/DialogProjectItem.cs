using System;
using System.IO;
using System.Linq;
using SysPath = System.IO.Path;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectItem : DialogProjectResourceObject, ISavable
    {
        public DialogProjectItem(DialogProjectResources resources, string filePath)
            : this(resources, GetResourceType(filePath), filePath)
        {
        }
        public DialogProjectItem(DialogProjectResources resources, DialogFileResourceType? type, string filePath)
            : this(resources, Guid.NewGuid(), type, filePath)
        {
        }
        public DialogProjectItem(DialogProjectResources resources, DialogProjectResourceItemSavedState savedState)
            : this(resources, Guid.Parse(savedState.ProjectId), savedState.ResourceType, SysPath.Combine(resources.Folder, savedState.FileName))
        {
            Id = savedState.Id;
        }
        private DialogProjectItem(DialogProjectResources resources, Guid id, DialogFileResourceType? type, string filePath)
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
            string expectedPath = SysPath.Combine(resources.Folder, fileName);

            if (filePath != expectedPath)
            {
                throw new ArgumentException($"Неверный путь к файлу", nameof(filePath));
            }

            FilePath = filePath;
            FileName = fileName;
            Type = type.GetValueOrDefault();
        }

        public override DialogResourceType ResourceType => DialogResourceType.File;
        public string FilePath { get; }
        public string FileName { get; }
        public DialogFileResourceType Type { get; }

        #region Управление

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectResourceItemSavedState
            {
                FileName = FileName,
                ResourceType = Type
            };
        }

        public override string ToString()
        {
            return $"[Файл {Type}] {Id}";
        }

        #endregion

        #region Константы

        public const string FilesFilter = "Все поддерживаемые форматы|*.ogg;*.mp3;*.mp4;*.jpg;*.jpeg;*.png|Аудиофайлы|*.ogg;*.mp3|Видео|*.mp4|Изображения|*.jpg;*.jpeg;*.png";

        #endregion

        #region Статика

        private static readonly string[] _audioExtensions = ["ogg", "mp3"];
        private static readonly string[] _videoExtensions = ["mp4"];
        private static readonly string[] _imageExtensions = ["png", "jpg", "jpeg"];

        public static DialogFileResourceType? GetResourceType(string filePath)
        {
            string extension = filePath.GetFileExtension();

            if (_audioExtensions.Contains(extension))
            {
                return DialogFileResourceType.Audio;
            }
            else if (_imageExtensions.Contains(extension))
            {
                return DialogFileResourceType.Image;
            }
            else if (_videoExtensions.Contains(extension))
            {
                return DialogFileResourceType.Video;
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
