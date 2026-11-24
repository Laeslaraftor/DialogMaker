namespace DialogMaker.Core.Scripting.Runtime.Numbers
{
    internal interface IDSharpNumberMathExecutor
    {
        /// <summary>
        /// left + right
        /// </summary>
        public DSharpNumber Add(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left - right
        /// </summary>
        public DSharpNumber Subtract(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left * right
        /// </summary>
        public DSharpNumber Multiply(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left / right
        /// </summary>
        public DSharpNumber Divide(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left % right
        /// </summary>
        public DSharpNumber Mod(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left ^ right
        /// </summary>
        public DSharpNumber Xor(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left << right
        /// </summary>
        public DSharpNumber ShiftLeft(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left >> right
        /// </summary>
        public DSharpNumber ShiftRight(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left & right
        /// </summary>
        public DSharpNumber And(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left | right
        /// </summary>
        public DSharpNumber Or(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left && right
        /// </summary>
        public DSharpNumber LogicalAnd(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left || right
        /// </summary>
        public DSharpNumber LogicalOr(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left == right
        /// </summary>
        public DSharpNumber LogicalEquals(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left != right
        /// </summary>
        public DSharpNumber LogicalNotEquals(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left < right
        /// </summary>
        public DSharpNumber LogicalLess(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left <= right
        /// </summary>
        public DSharpNumber LogicalLessOrEquals(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left > right
        /// </summary>
        public DSharpNumber LogicalGreater(ref DSharpNumber left, ref DSharpNumber right);
        /// <summary>
        /// left >= right
        /// </summary>
        public DSharpNumber LogicalGreaterOrEquals(ref DSharpNumber left, ref DSharpNumber right);

        /// <summary>
        /// -value
        /// </summary>
        public DSharpNumber Invert(ref DSharpNumber value);
        /// <summary>
        /// !value
        /// </summary>
        public DSharpNumber Not(ref DSharpNumber value);
        /// <summary>
        /// value++
        /// </summary>
        public DSharpNumber Increment(ref DSharpNumber value);
        /// <summary>
        /// value--
        /// </summary>
        public DSharpNumber Decrement(ref DSharpNumber value);
        /// <summary>
        /// ~value
        /// </summary>
        public DSharpNumber BitInvert(ref DSharpNumber value);
    }
}
