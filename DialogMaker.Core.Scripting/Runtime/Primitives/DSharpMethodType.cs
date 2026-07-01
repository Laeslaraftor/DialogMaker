namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type of method
    /// </summary>
    public enum DSharpMethodType
    {
        /// <summary>
        /// Default method. This is just regular method
        /// </summary>
        Default,
        /// <summary>
        /// Method for getting property value
        /// </summary>
        Getter,
        /// <summary>
        /// Method for setting property value 
        /// </summary>
        Setter,
        /// <summary>
        /// Object constructor
        /// </summary>
        Constructor,
        /// <summary>
        /// Finalizer/destructor
        /// </summary>
        Finalizer,
        /// <summary>
        /// Method that used as operator
        /// </summary>
        Operator
    }
}
