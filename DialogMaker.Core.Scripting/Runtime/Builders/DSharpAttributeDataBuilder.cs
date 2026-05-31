namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpAttributeDataBuilder(DSharpTypeBuilder type)
    {
        public DSharpTypeBuilder Type { get; set; } = type;
    }
}
