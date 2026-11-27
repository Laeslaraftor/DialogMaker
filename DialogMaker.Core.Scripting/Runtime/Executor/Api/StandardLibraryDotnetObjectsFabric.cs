using DialogMaker.Core.Scripting.Runtime.Executor.Api.DotnetWrappers;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Api
{
    internal class StandardLibraryDotnetObjectsFabric : IDSharpDotnetObjectsFabric
    {
        public DSharpDotnetObject? Create(int id, string typeName)
        {
            if (typeName == _stopwatch)
            {
                return new StopwatchWrapper(id);
            }

            return null;
        }

        private const string _stopwatch = "System.Diagnostics.Stopwatch";
    }
}
