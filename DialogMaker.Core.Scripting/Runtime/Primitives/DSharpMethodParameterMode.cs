namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Parameter mode
    /// </summary>
    public enum DSharpMethodParameterMode : byte
    {
        /// <summary>
        /// Default parameter
        /// </summary>
        Default,
        /// <summary>
        /// Method extension parameter
        /// </summary>
        This,
        /// <summary>
        /// Reference parameter. 
        /// It's directly refences to property, field, variable or other parameter
        /// </summary>
        Ref,
        /// <summary>
        /// Output parameter. 
        /// This parameter provides value after executing method or function
        /// </summary>
        Out
    }
}
