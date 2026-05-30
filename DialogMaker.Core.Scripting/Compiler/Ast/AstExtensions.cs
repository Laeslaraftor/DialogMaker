namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    internal static class AstExtensions
    {
        extension(DialogScriptPropertyAccessor accessor)
        {
            public DialogScriptPropertyAccessor Invert()
            {
                if (accessor == DialogScriptPropertyAccessor.Getter)
                {
                    return DialogScriptPropertyAccessor.Setter;
                }

                return DialogScriptPropertyAccessor.Getter;
            }
        }
    }
}
