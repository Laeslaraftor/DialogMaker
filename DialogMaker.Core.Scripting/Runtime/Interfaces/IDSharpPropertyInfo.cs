using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Interface of D# property
    /// </summary>
    public interface IDSharpPropertyInfo : IDSharpMemberInfo
    {
        /// <summary>
        /// Type of value that returns by current property
        /// </summary>
        public IDSharpType PropertyType { get; }
        /// <summary>
        /// Method that represents getter of current property
        /// </summary>
        public IDSharpMethodInfo? Getter { get; }
        /// <summary>
        /// Method that represents setter of current property
        /// </summary>
        public IDSharpMethodInfo? Setter { get; }
        /// <summary>
        /// Getter access modifier
        /// </summary>
        public DSharpAccessModifier? GetterAccess { get; }
        /// <summary>
        /// Setter access modifier
        /// </summary>
        public DSharpAccessModifier? SetterAccess { get; }
        /// <summary>
        /// Is current method supports reading
        /// </summary>
        public bool CanRead { get; }
        /// <summary>
        /// Is current method supports writing
        /// </summary>
        public bool CanWrite { get; }
        /// <summary>
        /// Property that was overriden by current property
        /// </summary>
        public IDSharpPropertyInfo? OverrideProperty { get; }
        /// <summary>
        /// Is property virtual. 
        /// Virtual properties can be overriden and have a implementation
        /// </summary>
        public bool IsVirtual { get; }
        /// <summary>
        /// Is property abstract. Abstract properties contains only in abstract classed,
        /// they can not have implementation
        /// </summary>
        public bool IsAbstract { get; }
        /// <summary>
        /// Is property sealed. Sealed properties can not be overriden
        /// </summary>
        public bool IsSealed { get; }
        /// <summary>
        /// Get array of interfaces property declarations that implemented by current property
        /// </summary>
        /// <returns>Array of interfaces property declarations that implemented by current property</returns>
        public IDSharpPropertyInfo[] GetImplementedProperties();
    }
}
