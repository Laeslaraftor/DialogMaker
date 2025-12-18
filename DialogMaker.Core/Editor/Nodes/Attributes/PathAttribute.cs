using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public sealed class PathAttribute(string path) : Attribute
    {
        public string Path { get; } = path;

        public string[] GetComponents()
        {
            return Path.Split('/');
        }
    }
}
