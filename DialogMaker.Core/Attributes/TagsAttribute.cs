namespace DialogMaker.Core
{
    /// <summary>
    /// Атрибут, задающий тэги
    /// </summary>
    /// <param name="tags">Тэги</param>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class TagsAttribute(params string[] tags) : Attribute
    {
        /// <summary>
        /// Создать атрибут, задающий тэги
        /// </summary>
        /// <param name="tags">Тэги, перечисленные через запятую</param>
        public TagsAttribute(string tags)
            : this(tags.Split(','))
        {
        }

        /// <summary>
        /// Массив тэгов
        /// </summary>
        public string[] Tags { get; } = tags;
    }
}
