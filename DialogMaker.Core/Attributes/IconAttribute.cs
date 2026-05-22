namespace DialogMaker.Core
{
    /// <summary>
    /// Атрибут, задающий иконку
    /// </summary>
    /// <param name="icon">Код иконки</param>
    public sealed class IconAttribute(string icon) : Attribute
    {
        /// <summary>
        /// Код иконки
        /// </summary>
        public string Icon { get; } = icon;
    }
}
