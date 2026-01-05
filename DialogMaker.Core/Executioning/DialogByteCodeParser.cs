using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DialogMaker.Core.Executioning
{
    public class DialogByteCodeParser
    {
        public DialogByteCodeParser(ReadOnlyCollection<Section> sections)
        {
            Sections = new(sections);
        }

        public ReadOnlyCollection<Section> Sections { get; }

        #region Статика

        public static DialogByteCodeParser Read(byte[] code, Dictionary<int, CodeSection> sections)
        {
            using MemoryStream stream = new(code);

            List<Section> sectionsCode = [];

            foreach (var section in sections)
            {
                List<Operation> operations = [];
                int endPosition = section.Value.EndPosition;

                stream.Position = section.Value.Position;

                while (endPosition > stream.Position)
                {
                    operations.Add(Operation.Read(stream));
                }

                sectionsCode.Add(new(section.Key, operations));
            }

            return new(new(sectionsCode));
        }

        #endregion

        #region Классы

        public struct Section(int index, List<Operation> operations)
        {
            public int Index { get; set; } = index;
            public ReadOnlyCollection<Operation> Operations { get; } = new(operations);
        }

        #endregion
    }
}
