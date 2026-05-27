using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning;
using MessagePack;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Common
{
    public class Dialog : ResourcesContainer, IDialogResourcesContainer
    {
        public Dialog(DialogFolder folder, DialogSavedState savedState)
        {
            Parent = folder;
            Id = savedState.Id;
            Name = savedState.Name;
            Folder = Path.Combine(folder.Folder, savedState.Id);
            Resources = DialogResources.Open(this);

            Dictionary<int, DialogItemReference> resourcesIndex = new(savedState.ResourcesIndex.Count);

            foreach (var info in savedState.ResourcesIndex)
            {
                resourcesIndex.Add(info.Key, DialogItemReference.Parse(info.Value));
            }

            _indexedResources = new(this, resourcesIndex);
            _bytecode = savedState.Bytecode;
        }
        private Dialog(DialogFolder folder, DialogProjectDialog dialog, DialogCompilerOutput compiledDialog)
        {
            Parent = folder;
            Id = dialog.Id;
            Name = dialog.Name;
            Folder = Path.Combine(folder.Folder, dialog.Id);
            Resources = new(this, dialog.Resources);

            _indexedResources = compiledDialog.Context.Build(this);

            using MemoryStream codeStream = new();
            compiledDialog.Write(codeStream);

            _bytecode = codeStream.ToArray();
        }

        public DialogPackage Package => Parent.Package;
        public DialogFolder Parent { get; }
        public string Id { get; }
        public string Name { get; }
        public string Folder { get; }
        public override DialogResources Resources { get; }

        IResourcesOwner IResourcesOwner.Root => Package;
        IResourcesOwner? IResourcesOwner.Parent => Parent;
        IResourcesContainer IResourcesOwner.Resources => Resources;

        private readonly DialogRuntimeResources _indexedResources;
        private readonly List<DialogExecutor> _executors = [];
        private readonly byte[] _bytecode;

        #region Управление

        public DialogExecutor CreateExecutor()
        {
            DialogExecutor executor = new(_bytecode, _indexedResources);
            executor.Disposed += OnExecutorDisposed;
            _executors.Add(executor);

            return executor;
        }
        public byte[] GetCode()
        {
            byte[] code = new byte[_bytecode.Length];
            ReadCode(code);

            return code;
        }
        public void ReadCode(byte[] buffer)
        {
            Array.Copy(_bytecode, buffer, Math.Min(buffer.Length, _bytecode.Length));
        }
        public void ReadCode(Stream stream)
        {
            stream.Write(_bytecode);
        }

        public void Save()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("Невозможно сохранить очищенный диалог");
            }

            var filePath = Path.Combine(Folder, $"{Id}.{FileExtension}");
            Dictionary<int, string> resourcesIndexes = new(_indexedResources.Items.Count);

            foreach (var info in _indexedResources.Items)
            {
                resourcesIndexes.Add(info.Key, info.Value.ToString());
            }

            DialogSavedState savedState = new()
            {
                Id = Id,
                Name = Name,
                Bytecode = _bytecode,
                ResourcesIndex = resourcesIndexes
            };

            FileExtensions.CreateDirectory(Folder);

            Resources.Save();
            savedState.Save(filePath);
        }

        public override bool TryFindChild(string id, [NotNullWhen(true)] out IResourcesOwner? result)
        {
            result = null;
            return false;
        }

        public override string ToString()
        {
            return $"[{nameof(Dialog)}: {Id}] {Name}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var executor in _executors)
            {
                executor.Disposed -= OnExecutorDisposed;
                executor.Dispose();
            }

            _executors.Clear();
            Resources.Dispose();
        }

        #endregion

        #region События

        private void OnExecutorDisposed(object sender, EventArgs e)
        {
            if (sender is not DialogExecutor executor)
            {
                return;
            }

            executor.Disposed -= OnExecutorDisposed;
            _executors.Remove(executor);
        }

        #endregion

        #region Константы

        public const string FileExtension = "dialog";

        #endregion

        #region Статика

        public static Dialog Create(DialogFolder folder, DialogProjectDialog dialog)
        {
            var compiler = DialogCompiler.Create(dialog);
            var compiledDialog = compiler.Compile();

            return new(folder, dialog, compiledDialog);
        }
        public static Dialog Open(DialogFolder folder, string id)
        {
            var filePath = Path.Combine(folder.Folder, id, $"{id}.{FileExtension}");
            var data = File.ReadAllBytes(filePath);
            var savedState = MessagePackSerializer.Deserialize<DialogSavedState>(data);

            return new(folder, savedState);
        }

        #endregion
    }
}
