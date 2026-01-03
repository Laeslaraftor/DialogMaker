using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning.Internal
{
    internal readonly struct ResourceString(string id, string text, IResourceFile? voice = null) : IResourceString
    {
        public ResourceString(int id, string text, IResourceFile? voice = null)
            : this(id.ToString(), text, voice)
        {
        }
        public ResourceString(string text, IResourceFile? voice = null)
            : this(string.Empty, text, voice)
        {
        }

        public string Id { get; } = id;
        public DialogResourceType ResourceType { get; } = DialogResourceType.String;
        public string Text { get; } = text;
        public IResourceFile? Voice { get; } = voice;

        #region Управление

        public override string ToString()
        {
            return $"[{Id}] {Text}";
        }

        #endregion
    }
}
