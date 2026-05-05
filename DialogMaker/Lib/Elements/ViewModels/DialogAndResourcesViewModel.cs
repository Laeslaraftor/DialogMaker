using DialogMaker.Core.Executioning.Debugging;
using DialogMaker.Editor;

namespace DialogMaker.Lib.Elements
{
    public class DialogAndResourcesViewModel : ObservableObject
    {
        public ProjectController? Project
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Project));
                    field = value;
                    OnPropertyChanged(nameof(Project));
                }
            }
        }
        public ProjectDialog? Dialog
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Dialog));
                    field = value;
                    OnPropertyChanged(nameof(Dialog));
                }
            }
        }
        public ProjectResources? Resources
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Resources));
                    field = value;
                    OnPropertyChanged(nameof(Resources));
                }
            }
        }
        public DialogCodeStructure? Structure
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Structure));
                    field = value;
                    OnPropertyChanged(nameof(Structure));
                }
            }
        }
    }
}
