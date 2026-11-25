namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type of D# math operation type
    /// </summary>
    public enum DSharpMathOperationType
    {
        /// <summary>
        /// Default math operation that uses stack values as operands
        /// </summary>
        Default,
        /// <summary>
        /// Math operation between values that stored in variables on both sides
        /// </summary>
        TwoVariables,
        /// <summary>
        /// Math operation between value that stored in variable (left) and field (right)
        /// </summary>
        VariableAndField,
        /// <summary>
        /// Math operation between value that stored in field (left) and variable (right)
        /// </summary>
        FieldAndVariable,
        /// <summary>
        /// Math operation between values that stored in fields on both sides.
        /// This operation may require object instances depends on fields
        /// </summary>
        TwoFields
    }
}
