namespace DialogMaker.Core.Scripting.Runtime.Numbers
{
    internal interface IDSharpNumberMathExecutor
    {
        /// <summary>
        /// left + right
        /// </summary>
        public DSharpNumber Add(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left - right
        /// </summary>
        public DSharpNumber Subtract(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left * right
        /// </summary>
        public DSharpNumber Multiply(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left / right
        /// </summary>
        public DSharpNumber Divide(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left % right
        /// </summary>
        public DSharpNumber Mod(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left ^ right
        /// </summary>
        public DSharpNumber Xor(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left << right
        /// </summary>
        public DSharpNumber ShiftLeft(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left >> right
        /// </summary>
        public DSharpNumber ShiftRight(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left & right
        /// </summary>
        public DSharpNumber And(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left | right
        /// </summary>
        public DSharpNumber Or(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left && right
        /// </summary>
        public DSharpNumber LogicalAnd(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left || right
        /// </summary>
        public DSharpNumber LogicalOr(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left == right
        /// </summary>
        public DSharpNumber LogicalEquals(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left != right
        /// </summary>
        public DSharpNumber LogicalNotEquals(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left < right
        /// </summary>
        public DSharpNumber LogicalLess(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left <= right
        /// </summary>
        public DSharpNumber LogicalLessOrEquals(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left > right
        /// </summary>
        public DSharpNumber LogicalGreater(DSharpNumber left, DSharpNumber right);
        /// <summary>
        /// left >= right
        /// </summary>
        public DSharpNumber LogicalGreaterOrEquals(DSharpNumber left, DSharpNumber right);

        /// <summary>
        /// -value
        /// </summary>
        public DSharpNumber Invert(DSharpNumber value);
        /// <summary>
        /// !value
        /// </summary>
        public DSharpNumber Not(DSharpNumber value);
        /// <summary>
        /// value++
        /// </summary>
        public DSharpNumber Increment(DSharpNumber value);
        /// <summary>
        /// value--
        /// </summary>
        public DSharpNumber Decrement(DSharpNumber value);
        /// <summary>
        /// ~value
        /// </summary>
        public DSharpNumber BitInvert(DSharpNumber value);
    }
}
