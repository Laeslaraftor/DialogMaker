using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core.Executioning.Builders
{
    public class DialogCodeBuilder
    {
        public DialogCodeBuilder(IResourcesOwner resourcesOwner)
        {
            ResourcesOwner = resourcesOwner;
            Sections = new(_sections);
        }

        public IResourcesOwner ResourcesOwner { get; }
        public ReferenceReadOnlyList<DialogSectionBuilder> Sections { get; }

        private readonly ObservableList<DialogSectionBuilder> _sections = [];

        #region Управление

        public int IndexOf(DialogSectionBuilder section) => _sections.IndexOf(section);

        public DialogSectionBuilder CreateSection()
        {
            DialogSectionBuilder result = new(this);
            _sections.Add(result);

            return result;
        }
        public bool RemoveSection(DialogSectionBuilder section)
        {
            return _sections.Remove(section);
        }

        public void Clear()
        {
            _sections.Clear();
        }

        public DialogCodeBuilderOutput Compile(Dictionary<INode, DialogCompilerNodeInfo> nodesInfo)
        {
            using MemoryStream codeStream = new();
            using MemoryStream tempCode = new();
            DialogExecutionContextBuilder contextBuilder = new(ResourcesOwner);
            CodeCompileContext context = new(nodesInfo, tempCode, contextBuilder);

            for (int i = 0; i < _sections.Count; i++)
            {
                tempCode.SetLength(0);
                _sections[i].Compile(context);

                var lengthData = BitConverter.GetBytes((int)tempCode.Length);
                codeStream.Write(lengthData);

                tempCode.Position = 0;
                tempCode.CopyTo(codeStream);
            }

            return new(codeStream.ToArray(), contextBuilder);
        }

        #endregion
    }
}
