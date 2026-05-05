using System.Collections;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogByteCodeData(DialogMetadata metadata, ReadOnlyCollection<DialogByteCodeData.Section> sections)
    {
        public DialogMetadata Metadata { get; } = metadata;
        public ReadOnlyCollection<Section> Sections { get; } = new(sections);

        #region Статика

        public static DialogByteCodeData Read(byte[] code)
        {
            using MemoryStream stream = new(code);
            return Read(stream);
        }
        public static DialogByteCodeData Read(Stream stream)
        {
            var metadata = DialogMetadata.Read(stream);

            List<Section> sectionsCode = [];
            int lastSectionEndPosition = 0;
            int sectionIndex = -1;
            int sectionPosition = 0;
            List<Operation> currentOperations = [];

            int ReadNumber()
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];

                for (int i = 0; i < buffer.Length; i++)
                {
                    var value = stream.ReadByte();

                    if (value == -1)
                    {
                        throw new InvalidDataException($"Не удалось прочитать число");
                    }

                    buffer[i] = (byte)value;
                }

                return BitConverter.ToInt32(buffer);
            }

            while (stream.Position < stream.Length)
            {
                if (0 >= lastSectionEndPosition)
                {
                    var length = ReadNumber();
                    lastSectionEndPosition += length;

                    if (currentOperations.Count > 0)
                    {
                        sectionsCode.Add(new(sectionIndex, sectionPosition, [.. currentOperations]));
                        currentOperations.Clear();
                    }

                    if (0 >= length)
                    {
                        continue;
                    }

                    sectionIndex++;
                    sectionPosition = (int)stream.Position;
                }

                int startPosition = (int)stream.Position;
                currentOperations.Add(Operation.Read(stream));
                lastSectionEndPosition -= (int)stream.Position - startPosition;
            }

            if (currentOperations.Count > 0)
            {
                sectionsCode.Add(new(sectionIndex, sectionPosition, currentOperations));
            }

            return new(metadata, new(sectionsCode));
        }

        #endregion

        #region Классы

        public readonly struct Section(int index, int position, List<Operation> operations) : IEnumerable<KeyValuePair<int, Operation>>
        {
            public int Index { get; } = index;
            public int Position { get; } = position;
            public ReadOnlyCollection<Operation> Operations { get; } = new(operations);

            public IEnumerator<KeyValuePair<int, Operation>> GetEnumerator()
            {
                int index = 0;

                foreach (var operation in Operations)
                {
                    yield return new(index, operation);
                    index++;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        #endregion
    }
}
