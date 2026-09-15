using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    internal static class AstExtensions
    {
        extension(IEnumerable<AstNode> nodes)
        {
            public void SetParent(AstNode? parent)
            {
                foreach (var node in nodes)
                {
                    node.Parent = parent;
                }
            }
        }
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
        extension(string value)
        {
            public string GetFileName(bool removeExtension = true)
            {
                value = value.Replace("/", @"\");
                var parts = value.Split('\\');
                value = parts[^1];

                if (removeExtension)
                {
                    var valueParts = value.Split('.');
                    
                    if (valueParts.Length > 1)
                    {
                        var lastPart = valueParts[^1];
                        value = value[..(value.Length - lastPart.Length - 1)];
                    }
                }

                return value;
            }
        }
    }
}
