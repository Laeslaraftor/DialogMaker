using Acly;
using System.Collections.Generic;
using System.IO;

namespace DialogMaker.Core.Executioning.Builders
{
    public class DialogCodeBuilder
    {
        public DialogCodeBuilder()
        {
            Sections = new(_sections);
        }

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

        public CompiledCodeInfo Compile()
        {
            using MemoryStream codeStream = new();
            Dictionary<int, CodeSection> sections = [];
            DialogExecutionContextBuilder contextBuilder = new();
            CodeCompileContext context = new(codeStream, contextBuilder);

            for (int i = 0; i < _sections.Count; i++)
            {
                int position = (int)codeStream.Length;
                _sections[i].Compile(context);

                int length = (int)codeStream.Length - position;
                sections.Add(i, new(position, length));
            }

            return new(codeStream.ToArray(), contextBuilder, sections);
        }

        #endregion
    }
}
