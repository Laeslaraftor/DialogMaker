using DialogMaker.Core;
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
                    InvokePropertyChanging(nameof(Project));
                    field = value;
                    InvokePropertyChanged(nameof(Project));
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
                    InvokePropertyChanging(nameof(Dialog));
                    field = value;
                    InvokePropertyChanged(nameof(Dialog));
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
                    InvokePropertyChanging(nameof(Resources));
                    field = value;
                    InvokePropertyChanged(nameof(Resources));
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
                    InvokePropertyChanging(nameof(Structure));
                    field = value;
                    InvokePropertyChanged(nameof(Structure));
                }
            }
        }
    }
}
