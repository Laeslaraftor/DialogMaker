using Acly;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DialogMaker.Core
{
    public class DialogProject
    {
        private DialogProject(string projectPath)
        {
            ProjectPath = projectPath;
            _packs = new();
            Packs = new(_packs);
        }

        public string ProjectPath { get; }
        public string Name { get; set; } = string.Empty;
        public ReferenceReadOnlyList<DialogProjectPack> Packs { get; }

        private readonly ObservableList<DialogProjectPack> _packs;

        #region Управление

        public bool TryGetPack(string id, [NotNullWhen(true)] out DialogProjectPack? result)
        {
            result = null;

            foreach (var pack in _packs)
            {
                if (pack.Id == id)
                {
                    result = pack;
                    return true;
                }
            }

            return false;
        }

        public DialogProjectPack CreatePack(string id, string name)
        {
            if (TryGetPack(id, out _))
            {
                throw new ArgumentException($"Набор диалогов с идентификатором {id} уже существует.", nameof(id));
            }

            DialogProjectPack pack = new(this, id)
            {
                Name = name
            };

            if (!Directory.Exists(pack.Folder))
            {
                Directory.CreateDirectory(pack.Folder);
            }

            _packs.Add(pack);

            return pack;
        }
        public bool RemovePack(DialogProjectPack pack)
        {
            return _packs.Remove(pack);
        }

        #endregion

        #region Статика

        public static DialogProject Open(string projectFilePath)
        {
            throw new NotImplementedException();
        }
        public static DialogProject Create(string name, string projectFolder)
        {
            return new(projectFolder)
            {
                Name = name
            };
        }

        #endregion
    }
}
