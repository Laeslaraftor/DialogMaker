using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DialogMaker.Editor
{
    public class ProjectFile : ProjectResourceItem<DialogProjectItem>
    {
        public ProjectFile(ProjectController project, DialogProjectItem original) : base(project, original)
        {
            Icon = original.Type.GetEnumAttribute<IconAttribute>()?.Icon ?? Icons.Unknown;
            FileUri = new(original.FilePath, UriKind.Absolute);
        }

        public Uri FileUri { get; }
        public string FilePath => Original.FilePath;
        public string FileName => Original.FileName;
        public DialogFileResourceType Type => Original.Type;
        public string Icon { get; }
        public FileView View
        {
            get
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Объект был очищен, доступ невозможен");
                }
                if (_view == null)
                {
                    _view = _viewsPool.GetElement();
                    _view.RemoveFromParent();
                    _view.File = this;
                }

                return _view;
            }
        }

        private readonly List<Image> _createdImages = [];
        private FileView? _view;

        #region Управление

        public object GetFilePreview()
        {
            if (Type != DialogFileResourceType.Image)
            {
                return Icon;
            }

            var image = _imagesPool.GetElement();
            image.RemoveFromParent();
            image.Stretch = Stretch.Uniform;
            image.Source = new BitmapImage(FileUri);

            _createdImages.Add(image);

            return image;
        }
        public void FreeFilePreview(object filePreview)
        {
            if (filePreview is Image image &&
                _viewsPool.Free(image))
            {
                image.Source = null;
                _createdImages.Remove(image);
            }
        }

        public override object? GetPreview()
        {
            var preview = _viewersPool.GetElement();
            preview.MediaFile = Original;

            return preview;
        }
        public override void FreePreview(object? preview)
        {
            if (preview is MediaViewer view && view.MediaFile?.Equals(Original) == true)
            {
                view.MediaFile = null;
                _viewersPool.Free(preview);
            }
        }

        public override ItemContextMenu CreateContextMenu()
        {
            return new FileContextMenu(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            
            if (_view != null)
            {
                _viewsPool.Free(_view);
                _view.RemoveFromParent();
                _view = null;
            }

            foreach (var image in _createdImages)
            {
                image.RemoveFromParent();
                _viewsPool.Free(image);
            }

            _createdImages.Clear();
        }

        #endregion

        #region Статика

        private readonly ElementsPool<Image> _imagesPool = new();
        private readonly ElementsPool<FileView> _viewsPool = new();
        private readonly ElementsPool<MediaViewer> _viewersPool = new();

        #endregion
    }
}
