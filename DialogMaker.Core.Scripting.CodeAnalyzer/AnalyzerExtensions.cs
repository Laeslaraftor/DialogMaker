using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    internal static class AnalyzerExtensions
    {
        extension(string value)
        {
            public string ToCamelCase()
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }

                return value[0].ToString().ToLower() + value.Substring(1);
            }
        }
        extension(ISymbol symbol)
        {
            public bool Compare(ISymbol? other)
            {
                return SymbolEqualityComparer.Default.Equals(symbol, other);
            }
        }
        extension(INamedTypeSymbol namedType)
        {
            public string FullName
            {
                get
                {
                    var @namespace = namedType.ContainingNamespace;
                    string result = namedType.Name;

                    if (@namespace != null)
                    {
                        result = @namespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + result;
                    }

                    return result;
                }
            }
            public string? Namespace => namedType.ContainingNamespace?.ToDisplayString();
        }
        extension<TKey, TValue>(ImmutableArray<KeyValuePair<TKey, TValue>> items)
        {
            public bool TryGetValue(TKey key, out TValue result)
            {
                foreach (var item in items)
                {
                    if (Equals(item.Key, key))
                    {
                        result = item.Value;
                        return true;
                    }
                }

                result = default!;
                return false;
            }
        }
        extension(TypedConstant typedConstant)
        {
            /// <summary>
            /// Returns the System.String that represents the current TypedConstant.
            /// </summary>
            /// <returns>A System.String that represents the current TypedConstant.</returns>
            public string? ToDisplayString()
            {
                if (typedConstant.IsNull)
                {
                    return "null";
                }
                if (typedConstant.Kind == TypedConstantKind.Array)
                {
                    return "{" + string.Join(", ", typedConstant.Values.Select(v => v.ToString())) + "}";
                }
                if (typedConstant.Kind == TypedConstantKind.Type || typedConstant.Type?.SpecialType == SpecialType.System_Object)
                {
                    return "typeof(" + typedConstant.Value?.ToString() + ")";
                }
                if (typedConstant.Kind == TypedConstantKind.Enum)
                {
                    var type = typedConstant.Type;

                    if (type == null)
                    {
                        return null;
                    }

                    string result = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    foreach (var field in type.GetMembers().Where(m => m is IFieldSymbol).Cast<IFieldSymbol>())
                    {
                        if (field.HasConstantValue && field.ConstantValue.Equals(typedConstant.Value))
                        {
                            result += "." + field.Name;
                            break;
                        }
                    }
                    
                    return result;
                }

                return SymbolDisplay.FormatPrimitive(typedConstant.Value, quoteStrings: true, useHexadecimalNumbers: false);
            }
        }
    }
}
