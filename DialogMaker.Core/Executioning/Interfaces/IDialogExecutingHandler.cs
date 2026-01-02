using DialogMaker.Core.Common;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutingHandler
    {
        public Task ShowReplica(ICharacter? character, string text);
        public Task ShowFullscreenReplica(ICharacter? character, IResourceItem? background, string text);
        public Task ShowColorReplica(ICharacter? character, Color backgroundColor, Color textColor, string text);
        public Task<int> ShowChoice(ICharacter? character, ReadOnlyCollection<string> variants);

        public Task HandleTrigger(string name);
    }
}
