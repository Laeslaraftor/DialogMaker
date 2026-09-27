using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    /// <summary>
    /// Static class with extensions for ast
    /// </summary>
    public static class AstExtensions
    {
        extension(AstNode node)
        {
            /// <summary>
            /// Check current node on containing other node
            /// </summary>
            /// <param name="other">Node for checking on containing</param>
            /// <returns>Is other node contains in current node</returns>
            public bool Contains(AstNode other)
            {
                var parent = other;

                while (parent != null)
                {
                    if (parent == node)
                    {
                        return true;
                    }

                    parent = parent.Parent;
                }

                return false;
            }
        }
        extension(IEnumerable<AstNode> nodes)
        {
            /// <summary>
            /// Set parent for all nodes
            /// </summary>
            /// <param name="parent">Parent that need to be setted for all nodes</param>
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
            /// <summary>
            /// Invert accessor. Getter -> Setter, Setter -> Getter
            /// </summary>
            /// <returns>Inverted accessor</returns>
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
            /// <summary>
            /// Convert access modifier to D# token
            /// </summary>
            /// <returns>D# token that equals to access modifier</returns>
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
            internal string GetFileName(bool removeExtension = true)
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
