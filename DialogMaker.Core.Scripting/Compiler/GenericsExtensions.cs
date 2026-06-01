using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler
{
    internal static class GenericsExtensions
    {
        extension(List<TypeInfoNode> types)
        {
            public string? GetGenericsName(bool simplify)
            {
                if (types.Count == 0)
                {
                    return null;
                }
                if (simplify)
                {
                    return $"`{types.Count}";
                }

                var result = "<";
                int count = 0;

                foreach (var type in types)
                {
                    if (count > 0)
                    {
                        result += ", ";
                    }

                    result += type.GetFullName(false, true);
                    count++;
                }

                return result + ">";
            }
        }
    }
}
