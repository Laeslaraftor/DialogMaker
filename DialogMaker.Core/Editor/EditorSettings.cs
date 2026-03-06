namespace DialogMaker.Core.Editor
{
    public static class EditorSettings
    {
        public static ILogger Logger { get; set; } = ConsoleLogger.Instance;
    }
}
