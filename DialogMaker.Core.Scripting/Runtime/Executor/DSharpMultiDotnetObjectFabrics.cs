namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Class that unions multiply .NET object fabrics
    /// </summary>
    public class DSharpMultiDotnetObjectFabrics : IDSharpDotnetObjectsFabric
    {
        /// <summary>
        /// List or .NET object fabrics that will be used for creating wrappers
        /// </summary>
        public List<IDSharpDotnetObjectsFabric> Fabrics { get; } = [];

        public DSharpDotnetObject? Create(int id, string typeName)
        {
            foreach (var fabric in Fabrics)
            {
                var obj = fabric.Create(id, typeName);

                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }
    }
}
