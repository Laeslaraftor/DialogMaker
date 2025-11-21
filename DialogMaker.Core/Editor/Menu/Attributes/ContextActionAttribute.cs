using System;
using System.Collections.Generic;
using System.Drawing;

namespace DialogMaker.Core.Editor
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ContextActionAttribute : Attribute
    {
        public ContextActionAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Color? Color { get; set; }
        /// <summary>
        /// Func<object?, bool>
        /// </summary>
        public string? CanExecuteMethod { get; set; }
        /// <summary>
        /// Func<IEnumerable<ContextActionAttribute>>
        /// </summary>
        public string? Subitems { get; set; }
    }
}
