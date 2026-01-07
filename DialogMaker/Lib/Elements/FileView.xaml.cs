using DialogMaker.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DialogMaker.Lib.Elements
{
    public partial class FileView : UserControl, IDisposable
    {
        public FileView()
        {
            InitializeComponent();
            _idEditor.EditCommand = ProjectResourceItem.IdEditCommand;
        }
        ~FileView()
        {
            Dispatcher.Invoke(Dispose);
        }

        public ProjectFile? File
        {
            get => GetValue(FileProperty) as ProjectFile;
            set => SetValue(FileProperty, value);
        }

        #region Управление

        private void SetFile(ProjectFile? oldValue, ProjectFile? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            _preview.MediaFile = newValue?.Original;
            _idEditor.DataContext = newValue;
            _idEditor.EditCommandParameter = newValue;
            _flagsView.Value = newValue?.Original.Resources.Flags;
            _fileTypeView.Value = newValue?.Type;
            ContextMenu = newValue?.ContextMenu;
        }

        public void Dispose()
        {
            File = null;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region События

        private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView view)
            {
                view.SetFile(e.OldValue as ProjectFile, e.NewValue as ProjectFile);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(nameof(File), typeof(ProjectFile),
            typeof(FileView), new(OnFileChanged));

        #endregion
    }
}
