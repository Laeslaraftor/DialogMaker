namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpMethodBuilderParameter(DSharpAssemblyBuilder assembly) : IDSharpParameterInfo
    {
        public DSharpAssemblyBuilder Assembly { get; } = assembly;
        public string? Name { get; set; }
        public DSharpTypeToken? Type { get; set; }

        string IDSharpParameterInfo.Name => Name ?? string.Empty;
        IDSharpType IDSharpParameterInfo.Type
        {
            get
            {
                if (Type == null)
                {
                    throw new InvalidOperationException("Parameter type was not specified");
                }

                return (IDSharpType)Assembly.GetType(Type);
            }
        }
    }
}
