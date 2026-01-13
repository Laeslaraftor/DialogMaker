using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogMetadata(IDictionary<DialogExecutionEvent, ReadOnlyCollection<int>> eventSections, IDictionary<int, DialogItemReference> localValues)
    {
        public ReadOnlyDictionary<DialogExecutionEvent, ReadOnlyCollection<int>> EventSections { get; } = new(eventSections);
        public ReadOnlyDictionary<int, DialogItemReference> LocalValues { get; } = new(localValues);

        #region Управление

        public void Write(Stream stream)
        {
            void WriteNumber(int number)
            {
                var bytes = BitConverter.GetBytes(number);
                stream.Write(bytes);
            }
            void WriteText(string text)
            {
                var bytes = Encoding.Unicode.GetBytes(text);
                stream.Write(bytes);
            }

            stream.Write(MagicBytes);
            WriteNumber(EventSections.Count);

            foreach (var info in EventSections)
            {
                if (info.Value.Count == 0)
                {
                    continue;
                }

                stream.WriteByte((byte)info.Key);
                WriteNumber(info.Value.Count);

                foreach (var section in info.Value)
                {
                    WriteNumber(section);
                }
            }

            WriteNumber(LocalValues.Count);

            foreach (var info in LocalValues)
            {
                string value = info.Value.ToString();
                WriteNumber(info.Key);
                WriteNumber(value.Length * sizeof(char));
                WriteText(value);
            }
        }

        #endregion

        #region Константы

        public const string Magic = "dmbc"; // Dialog Maker bytecode

        #endregion

        #region Статика

        private static byte[] MagicBytes
        {
            get
            {
                field ??= Encoding.Unicode.GetBytes(Magic);
                return field;
            }
        }

        public static DialogMetadata Read(Stream dialogStream)
        {
            var magicBytes = MagicBytes;
            Span<byte> magicSpan = stackalloc byte[magicBytes.Length];

            int bytesRead = dialogStream.Read(magicSpan);

            if (bytesRead != magicSpan.Length)
            {
                throw new InvalidDataException($"Не удалось прочитать магическое значение. Прочтено байтов: {bytesRead}, требуется: {magicSpan.Length}");
            }
            for (int i = 0; i < magicSpan.Length; i++)
            {
                if (magicBytes[i] != magicSpan[i])
                {
                    throw new ArgumentException($"Неверное магическое значение образа!", nameof(dialogStream));
                }
            }

            Span<byte> numberBuffer = stackalloc byte[sizeof(int)];

            int ReadInt(Span<byte> buffer)
            {
                int readCount = dialogStream.Read(buffer);

                if (readCount != buffer.Length)
                {
                    throw new ArgumentException($"Не удалось прочитать значение. Прочтено: {readCount}, требуется: {buffer.Length}", nameof(dialogStream));
                }

                return BitConverter.ToInt32(buffer);
            }

            int eventSectionsCount = ReadInt(numberBuffer);
            Dictionary<DialogExecutionEvent, IList<int>> eventSections = new(eventSectionsCount);

            for (int i = 0; i < eventSectionsCount; i++)
            {
                var eventValue = dialogStream.ReadByte();

                if (eventValue == -1)
                {
                    throw new ArgumentException("Не удалось прочитать тип события сегментов");
                }

                int valuesCount = ReadInt(numberBuffer);
                List<int> sections = new(valuesCount);

                for (int s = 0; s < valuesCount; s++)
                {
                    sections.Add(ReadInt(numberBuffer));
                }

                eventSections.Add((DialogExecutionEvent)eventValue, sections);
            }

            int localValuesCount = ReadInt(numberBuffer);
            Dictionary<int, DialogItemReference> localValues = new(localValuesCount);
            byte[]? referenceBytesBuffer = null;

            for (int i = 0; i < localValuesCount; i++)
            {
                int index = ReadInt(numberBuffer);
                int valueBufferLength = ReadInt(numberBuffer);

                if (referenceBytesBuffer == null ||
                    valueBufferLength > referenceBytesBuffer.Length)
                {
                    referenceBytesBuffer = new byte[valueBufferLength];
                }

                dialogStream.Read(referenceBytesBuffer, 0, valueBufferLength);

                string textReference = Encoding.Unicode.GetString(referenceBytesBuffer, 0, valueBufferLength);
                var reference = DialogItemReference.Parse(textReference);

                localValues.Add(index, reference);
            }

            return new(eventSections.ToReadonly(), localValues);
        }

        #endregion
    }
}
