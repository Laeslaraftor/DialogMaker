using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Lib;
using System.ComponentModel;

namespace DialogMaker.Editor.Data
{
    public class ProjectItemCreationInfo : ObservableObject
    {
        [Name("Идентификатор"), Description("Идентификатор элемента. Папка или файл объекта будет названа идентификатором элемента")]
        public string Id
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Id));
                    field = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        [Name("Название"), Description("Название элемента")]
        public string Name
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Name));
                    field = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        #region Статика

        public static async Task<ProjectItemCreationInfo?> Create(string title, string? itemName)
        {
            Validator validator = Validator.Instance;
            ProjectItemCreationInfo result = new();

            if (itemName != null)
            {
                validator = new(itemName);
            }

            bool isConfirmed = await Alerts.Fill(title, result, validator);

            if (isConfirmed)
            {
                return result;
            }

            return null;
        }

        #endregion

        #region Классы

        public class Validator(string itemName) : IFillerValidator<ProjectItemCreationInfo>
        {
            public string ItemName { get; } = itemName;

            public bool Validate(ProjectItemCreationInfo item)
            {
                if (string.IsNullOrEmpty(item.Id?.Trim()))
                {
                    Alerts.Show("Недопустимый идентификатор", $"Невозможно создать {ItemName}, так как идентификатор не указан");
                    return false;
                }
                if (string.IsNullOrEmpty(item.Name?.Trim()))
                {
                    Alerts.Show("Недопустимое название", $"Невозможно создать {ItemName}, так как название не указано");
                    return false;
                }

                return true;
            }

            public static readonly Validator Instance = new("элемент");
        }

        #endregion
    }
}
