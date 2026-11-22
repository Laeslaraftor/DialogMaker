using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using System.Diagnostics.CodeAnalysis;

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
        extension(DSharpLiteralType literalType)
        {
            /// <summary>
            /// Get token type that represents by current literal type
            /// </summary>
            /// <returns>Token type</returns>
            /// <exception cref="ArgumentException">Invalid literal type</exception>
            public DSharpTokenType ToToken()
            {
                return literalType switch
                {
                    DSharpLiteralType.Null => DSharpTokenType.Null,
                    DSharpLiteralType.Byte => DSharpTokenType.Byte,
                    DSharpLiteralType.SByte => DSharpTokenType.SByte,
                    DSharpLiteralType.Bool => DSharpTokenType.Bool,
                    DSharpLiteralType.Short => DSharpTokenType.Short,
                    DSharpLiteralType.UShort => DSharpTokenType.UShort,
                    DSharpLiteralType.Char => DSharpTokenType.Char,
                    DSharpLiteralType.Int => DSharpTokenType.Int,
                    DSharpLiteralType.UInt => DSharpTokenType.UInt,
                    DSharpLiteralType.Long => DSharpTokenType.Long,
                    DSharpLiteralType.ULong => DSharpTokenType.ULong,
                    DSharpLiteralType.NInt => DSharpTokenType.Nint,
                    DSharpLiteralType.NUInt => DSharpTokenType.Nuint,
                    DSharpLiteralType.Float => DSharpTokenType.Float,
                    DSharpLiteralType.Double => DSharpTokenType.Double,
                    DSharpLiteralType.Decimal => DSharpTokenType.Decimal,
                    DSharpLiteralType.String => DSharpTokenType.String,
                    _ => throw new ArgumentException($"Invalid literal type: {literalType}", nameof(literalType))
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

        /// <summary>
        /// Check current node on System.Void type
        /// </summary>
        /// <param name="typeInfoNode">Type node or null</param>
        /// <returns>Returns true when specified node is null or System.Void</returns>
        public static bool IsVoid([NotNullWhen(true)] this TypeInfoNode? typeInfoNode)
        {
            if (typeInfoNode == null)
            {
                return true;
            }

            var fullName = typeInfoNode.GetFullName(true, false);

            return fullName == DSharpBuildInTypes.Void.ShortName ||
                   fullName == DSharpBuildInTypes.Void.FullName;
        }
    }
}
