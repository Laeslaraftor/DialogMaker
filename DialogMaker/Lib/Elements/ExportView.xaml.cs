using DialogMaker.Core;
using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;
using DialogMaker.Editor;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class ExportView : UserControl
    {
        public ExportView()
        {
            InitializeComponent();
            _outputList.ItemsSource = _log;
        }

        public ProjectController? ProjectController
        {
            get => GetValue(ProjectControllerProperty) as ProjectController;
            set => SetValue(ProjectControllerProperty, value);
        }

        private string OutputPath
        {
            get => _outputPathEntry.Text;
            set
            {
                _outputPathEntry.Text = value;
                bool isCorrect = false;

                try
                {
                    isCorrect = Directory.Exists(value);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }

                _pathErrorView.Visibility = isCorrect ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private bool IsExporting
        {
            get => !_exportButton.IsEnabled;
            set => _exportButton.IsEnabled = !value;
        }

        private readonly ObservableCollection<CompileLog> _log = [];
        private readonly ElementsPool<CompileLog> _logsPool = new();

        #region Управление

        public void Clear()
        {
            _log.Clear();
            _logsPool.Clear();
        }

        private async void StartExport()
        {
            if (IsExporting)
            {
                return;
            }

            var project = ProjectController;
            var outputPath = OutputPath;

            if (project == null)
            {
                Alerts.Show("Экспорт невозможен", "Проект для экспорта не был указан");
                return;
            }
            if (!Directory.Exists(outputPath))
            {
                Alerts.Show("Экспорт невозможен", "Указан неверный путь к папке экспорта");
                return;
            }

            IsExporting = true;
            Clear();

            var builder = DialogPackage.CreateWithProgress(project.Project, outputPath);
            var dispatcher = Dispatcher;
            HashSet<object> compiledObjects = [];
            var package = await Task.Run(() =>
            {
                DialogPackage? package = null;

                try
                {
                    foreach (var result in builder)
                    {
                        package = result.Value;

                        dispatcher.Invoke(() =>
                        {
                            SetProgress(result);
                        });
                    }
                }
                catch (Exception error)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var log = _logsPool.GetElement();
                        log.Prefix = $"{error.GetType().Name}:";
                        log.Item = error.Message;
                        log.Progress = 0;
                        log.IsError = true;

                        _log.Add(log);
                    });
                }

                return package;
            });

            package?.Save();
            IsExporting = false;
        }

        private void SetProgress(ProgressResult<DialogPackage> progressResult)
        {
            var progress = progressResult.Progress * 100;
            _exportProgressBar.Value = progress;
            _exportProgressPercent.Text = Math.Round(progress) + "%";

            int localProgress = (int)Math.Round(progressResult.LocalProgress * 100);

            if (progressResult.Extra != null)
            {
                for (int i = 0; i < _log.Count; i++)
                {
                    var logItem = _log[i];

                    if (logItem.Item == null)
                    {
                        continue;
                    }
                    if (logItem.Item.Equals(progressResult.Extra))
                    {
                        logItem.Progress = localProgress;
                        return;
                    }
                }
            }

            string prefix = "Сборка";

            if (progressResult.Extra is DialogProjectDialog)
            {
                prefix = "Сборка диалога";
            }
            else if (progressResult.Extra is DialogProjectPack)
            {
                prefix = "Сборка набора";
            }
            else if (progressResult.Extra is DialogProject)
            {
                prefix = "Сборка проекта";
            }

            var log = _logsPool.GetElement();
            log.Prefix = prefix;
            log.Item = progressResult.Extra;
            log.Progress = localProgress;
            log.IsError = false;

            _log.Add(log);
        }

        #endregion

        #region События

        private void OnPathEntryConfirmedText(object sender, ValueChangedEventArgs<string> e)
        {
            OutputPath = e.NewValue;
        }

        private void OnSelectFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new()
            {
                Title = "Выбрать папку для экспорта проекта"
            };

            if (folderDialog.ShowDialog() != true)
            {
                return;
            }

            OutputPath = folderDialog.FolderName;
        }
        private void OnOpenSelectedFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            var folder = OutputPath;

            if (Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = folder,
                    UseShellExecute = true,
                });
            }
        }
        private void OnExportButtonClicked(object sender, RoutedEventArgs e)
        {
            StartExport();
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty ProjectControllerProperty = DependencyProperty.Register(nameof(ProjectController), typeof(ProjectController),
            typeof(ExportView));

        #endregion

        #region Классы

        private class CompileLog : ObservableObject
        {
            public CompileLog()
            {
                NotStarted = true;
            }

            public string Prefix
            {
                get => field ?? string.Empty;
                set
                {
                    if (field != value)
                    {
                        field = value;
                        InvokePropertyChanged(nameof(Prefix));
                    }
                }
            }
            public object? Item
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        field = value;
                        InvokePropertyChanged(nameof(Item));
                    }
                }
            }
            public int Progress
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        field = value;
                        NotStarted = value <= 0;
                        InvokePropertyChanged(nameof(Progress));
                    }
                }
            }
            public bool NotStarted
            {
                get => field;
                private set
                {
                    if (field != value)
                    {
                        field = value;
                        InvokePropertyChanged(nameof(NotStarted));
                    }
                }
            }
            public bool IsError
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        field = value;
                        InvokePropertyChanged(nameof(IsError));
                    }
                }
            }

            public void Clear()
            {
                Prefix = string.Empty;
                Item = null;
                Progress = 0;
            }
        }

        #endregion
    }
}
