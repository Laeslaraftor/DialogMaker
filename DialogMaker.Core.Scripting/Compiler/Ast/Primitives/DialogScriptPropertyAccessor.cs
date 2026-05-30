using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DialogScriptPropertyAccessor
    {
        Getter = DialogScriptTokenType.Get,
        Setter = DialogScriptTokenType.Set,
    }
}
