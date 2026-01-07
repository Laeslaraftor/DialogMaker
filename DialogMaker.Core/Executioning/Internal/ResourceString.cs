using DialogMaker.Core.Common;
using System;
using System.Diagnostics;

namespace DialogMaker.Core.Executioning.Internal
{
    internal class ResourceString(string id, string text, IResourceFile? voice = null) : IResourceString
    {
        public ResourceString(int id, string text, IResourceFile? voice = null)
            : this(id.ToString(), text, voice)
        {
        }
        public ResourceString(string text, IResourceFile? voice = null)
            : this(string.Empty, text, voice)
        {
        }
        public ResourceString(int id, string text, ResourcePath voicePath, IResourcesOwner resourcesOwner)
            : this(id.ToString(), text, null)
        {
            _resourcesOwner = resourcesOwner;
            _voicePath = voicePath;
        }
        public ResourceString(string text, ResourcePath voicePath, IResourcesOwner resourcesOwner)
            : this(string.Empty, text, null)
        {
            _resourcesOwner = resourcesOwner;
            _voicePath = voicePath;
        }

        public string Id { get; } = id;
        public DialogResourceType ResourceType { get; } = DialogResourceType.String;
        public string Text { get; } = text;
        public IResourceFile? Voice
        {
            get
            {
                if (_voice != null)
                {
                    return _voice;
                }
                if (_triedToFindVoice ||
                    _voicePath == ResourcePath.Empty || 
                    _resourcesOwner == null)
                {
                    return null;
                }

                try
                {
                    _voice = IResourcesOwner.FindResource(_resourcesOwner, _voicePath) as IResourceFile;
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }

                _triedToFindVoice = true;

                return _voice;
            }
        }
        public bool IsSeparated => true;

        private readonly IResourcesOwner? _resourcesOwner;
        private readonly ResourcePath _voicePath = ResourcePath.Empty;
        private IResourceFile? _voice = voice;
        private bool _triedToFindVoice;

        #region Управление

        public DialogItemReference CreateReference()
        {
            return DialogItemReference.Create(this);
        }
        public ResourcePath GetPath()
        {
            throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
        }

        public override string ToString()
        {
            return $"[{Id}] {Text}";
        }

        #endregion
    }
}
