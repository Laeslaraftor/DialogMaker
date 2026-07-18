namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Class that unites external methods providers and search requested method in specified providers
    /// </summary>
    public class DSharpMultiExternalMethodsProviders : IDSharpExternalMethodsProvider
    {
        /// <summary>
        /// Providers that used to provide external methods
        /// </summary>
        public List<IDSharpExternalMethodsProvider> Providers { get; } = [];

        public DSharpExternalMethod? GetMethod(IDSharpMethodInfo methodInfo)
        {
            foreach (var provider in Providers)
            {
                var method = provider.GetMethod(methodInfo);

                if (method != null)
                {
                    return method;
                }
            }

            return null;
        }
    }
}
