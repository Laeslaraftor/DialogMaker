using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public enum DialogScriptAccessModifier
    {
        Public = DialogScriptTokenType.Public,
        Private = DialogScriptTokenType.Private,
        Protected = DialogScriptTokenType.Protected
    }
}
