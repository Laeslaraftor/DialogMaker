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
    }
}
