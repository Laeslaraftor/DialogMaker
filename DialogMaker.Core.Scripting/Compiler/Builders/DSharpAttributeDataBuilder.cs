namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public class DSharpAttributeDataBuilder(DSharpTypeBuilder type)
    {
        public DSharpTypeBuilder Type { get; set; } = type;
    }
}
