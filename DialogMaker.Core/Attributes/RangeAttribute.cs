namespace DialogMaker.Core
{
    /// <summary>
    /// Атрибут, задающий минимальное и максимальное допустимое значение 
    /// </summary>
    /// <param name="minimum">Минимальное допустимое значение</param>
    /// <param name="maximum">Максимальное допустимое значение</param>
    public sealed class RangeAttribute(float minimum, float maximum) : Attribute
    {
        /// <summary>
        /// Минимальное допустимое значение
        /// </summary>
        public float Minimum { get; } = minimum;
        /// <summary>
        /// Максимальное допустимое значение
        /// </summary>
        public float Maximum { get; } = maximum;
    }
}
