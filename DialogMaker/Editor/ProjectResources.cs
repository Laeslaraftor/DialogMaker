using Acly;
using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using System.Windows.Input;

namespace DialogMaker.Editor
{
    public class ProjectResources : IDisposable
    {
        public ProjectResources(ProjectController controller, DialogProjectResources resources)
        {
            Controller = controller;
            Original = resources;

            _stringsConverter = new(controller);

            ObservableList<ProjectString> strings = [];
            _strings = new(resources.Strings, strings, _stringsConverter);
            Strings = new(strings);
            CreateStringCommand = new RelayCommand(ExecuteCreateString);
        }

        public ProjectController Controller { get; }
        public DialogProjectResources Original { get; }
        public ReferenceReadOnlyList<ProjectString> Strings { get; }
        public ICommand CreateStringCommand { get; }

        private readonly ProjectStringConverter _stringsConverter;
        private readonly CollectionSynchronizer<DialogProjectString, ProjectString> _strings;

        #region Управление

        public void Dispose()
        {
            _strings.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Команды

        private void ExecuteCreateString(object? parameter)
        {
            try
            {
                Original.CreateString();
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion
    }
}
