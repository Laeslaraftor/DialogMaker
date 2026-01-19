namespace DialogMaker.Core
{
    public struct ProgressResult<T>
    {
        public bool IsCompleted { get; set; }
        public float Progress { get; set; }
        public float LocalProgress { get; set; }
        public T Value { get; set; }
        public object? Extra { get; set; }
    }
}
