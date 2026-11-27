using System.Diagnostics;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Api.DotnetWrappers
{
    internal class StopwatchWrapper(int id) : DSharpDotnetObject(id)
    {
        private readonly Stopwatch _stopwatch = new();

        #region Controls

        protected override DSharpExternalMethod? GetMethodDelegate(IDSharpMethodInfo methodInfo)
        {
            return methodInfo.Name switch
            {
                nameof(GetIsRunning) => GetIsRunning,
                nameof(GetElapsedMilliseconds) => GetElapsedMilliseconds,
                nameof(GetElapsedTicks) => GetElapsedTicks,
                nameof(Start) => Start,
                nameof(Restart) => Restart,
                nameof(Reset) => Reset,
                nameof(Stop) => Stop,
                _ => null
            };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stopwatch.Stop();
        }

        #endregion

        #region Methods

        private DSharpExternalMethodResult? GetIsRunning(DSharpExternalCallingArgs _)
        {
            return _stopwatch.IsRunning;
        }
        private DSharpExternalMethodResult? GetElapsedMilliseconds(DSharpExternalCallingArgs _)
        {
            return _stopwatch.ElapsedMilliseconds;
        }
        private DSharpExternalMethodResult? GetElapsedTicks(DSharpExternalCallingArgs _)
        {
            return _stopwatch.ElapsedTicks;
        }

        private DSharpExternalMethodResult? Start(DSharpExternalCallingArgs _)
        {
            _stopwatch.Start();
            return null;
        }
        private DSharpExternalMethodResult? Restart(DSharpExternalCallingArgs _)
        {
            _stopwatch.Restart();
            return null;
        }
        private DSharpExternalMethodResult? Reset(DSharpExternalCallingArgs _)
        {
            _stopwatch.Reset();
            return null;
        }
        private DSharpExternalMethodResult? Stop(DSharpExternalCallingArgs _)
        {
            _stopwatch.Stop();
            return null;
        }

        #endregion
    }
}
