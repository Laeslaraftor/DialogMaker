using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using Microsoft.Win32;
using System.Collections;
using System.Windows.Controls;

namespace DialogMaker.Editor
{
    public partial class ProjectController
    {
        public static DialogProject? Create()
        {
            var id = Alerts.RequestText("Введите идентификатор нового проекта", "Создать");

            if (id == null)
            {
                return null;
            }

            OpenFolderDialog folderDialog = new()
            {
                Title = "Выберите папку для создания проекта",
                Multiselect = false
            };

            if (folderDialog.ShowDialog() == true)
            {
                return DialogProject.Create(id, folderDialog.FolderName); 
            }

            return null;
        }
        public static DialogProject? Open()
        {
            OpenFileDialog fileDialog = new()
            {
                Title = "Выберите проект диалогов",
                Filter = DialogProject.FileFilter,
                Multiselect = false
            };

            if (fileDialog.ShowDialog() == true)
            {
                return DialogProject.Open(fileDialog.FileName);
            }

            return null;
        }
    }
}
