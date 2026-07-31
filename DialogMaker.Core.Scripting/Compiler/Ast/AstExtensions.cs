using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    internal static class AstExtensions
    {
        extension(DSharpPropertyAccessor accessor)
        {
            public DSharpPropertyAccessor Invert()
            {
                if (accessor == DSharpPropertyAccessor.Getter)
                {
                    return DSharpPropertyAccessor.Setter;
                }

                return DSharpPropertyAccessor.Getter;
            }
        }
        extension(DSharpAccessModifier access)
        {
            public DSharpTokenType ToToken()
            {
                return access switch
                {
                    DSharpAccessModifier.Public => DSharpTokenType.Public,
                    DSharpAccessModifier.Protected => DSharpTokenType.Protected,
                    DSharpAccessModifier.Private => DSharpTokenType.Private,
                    DSharpAccessModifier.Internal => DSharpTokenType.Internal,
                    _ => DSharpTokenType.Public,
                };
            }
        }
    }
}
