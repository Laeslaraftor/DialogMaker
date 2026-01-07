using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogByteCodeData(ReadOnlyCollection<DialogByteCodeData.Section> sections)
    {
        public ReadOnlyCollection<Section> Sections { get; } = new(sections);

        #region Статика

        public static DialogByteCodeData Read(byte[] code)
        {
            using MemoryStream stream = new(code);

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

            return new(new(sectionsCode));
        }

        #endregion

        #region Классы

        public readonly struct Section(int index, int position, List<Operation> operations)
        {
            public int Index { get; } = index;
            public int Position { get; } = position;
            public ReadOnlyCollection<Operation> Operations { get; } = new(operations);
        }

        #endregion
    }
}
