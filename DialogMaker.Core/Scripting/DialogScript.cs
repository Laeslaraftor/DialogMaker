using DialogMaker.Core.Scripting.Tokens;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Scripting
{
    public class DialogScript(IList<DialogScriptToken> tokens)
    {
        public ReadOnlyCollection<DialogScriptToken> Tokens { get; } = new(tokens);
    }
}
