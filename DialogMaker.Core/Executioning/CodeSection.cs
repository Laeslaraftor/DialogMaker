namespace DialogMaker.Core.Executioning
{
    public readonly struct CodeSection(int position, int length)
    {
        public int Position { get; } = position;
        public int Length { get; } = length;
        public int EndPosition => Position + Length;    
    }
}
