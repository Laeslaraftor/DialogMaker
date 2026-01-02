using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DialogMaker.Core.Executioning
{
    public struct Operation
    {
        public Operation(DialogByteCode code)
        {
            Code = code;

            var argsCount = CodeArguments[code];
            Arguments = argsCount > 0 ? new int[argsCount] : [];
        }

        public DialogByteCode Code { get; }
        public int[] Arguments { get; }

        #region Статика

        private static ReadOnlyDictionary<DialogByteCode, int> CodeArguments
        {
            get
            {
                if (field == null)
                {
                    Dictionary<DialogByteCode, int> args = [];

                    foreach (var value in Enum.GetValues(typeof(DialogByteCode)))
                    {
                        var argsAttribute = value.GetEnumAttribute<ArgsCountAttribute>();
                        int count = 0;

                        if (argsAttribute != null)
                        {
                            count = (int)argsAttribute.ArgumentsCount;
                        }

                        args.Add((DialogByteCode)value, count);
                    }

                    field = new(args);
                }

                return field;
            }
        }
        private static readonly Dictionary<DialogByteCode, OpCode> _opCodeInstances = [];

        public static OpCode GetImplementation(DialogByteCode code)
        {
            if (_opCodeInstances.TryGetValue(code, out var instance))
            {
                var attr = code.GetEnumAttribute<ImplementationAttribute>();

                if (attr == null)
                {
                    throw new ArgumentException($"Атрибут реализации не указан для {code}!", nameof(code));
                }

                instance = attr.GetInstance();
                _opCodeInstances.Add(code, instance);
            }

            return instance;
        }
        public static Operation Read(Stream codeStream)
        {
            var opcodeValue = codeStream.ReadByte();

            if (opcodeValue == -1)
            {
                throw new ArgumentException($"Не удалось прочитать значение оператора, получено значение: {opcodeValue}");
            }

            var opCode = (DialogByteCode)opcodeValue;
            Operation result = new(opCode);
            Span<byte> buffer = stackalloc byte[sizeof(int)];

            for (int i = 0; i < result.Arguments.Length; i++)
            {
                int bytesRead = 0;

                for (int v = 0; v < buffer.Length; i++)
                {
                    var value = codeStream.ReadByte();

                    if (value == -1)
                    {
                        break;
                    }

                    buffer[v] = (byte)value;
                    bytesRead++;
                }

                if (bytesRead != buffer.Length)
                {
                    throw new ArgumentException($"Не удалось прочитать значение аргумента {i}. Требуемое количество байтов: {buffer.Length}, прочтено: {bytesRead}", nameof(codeStream));
                }

                result.Arguments[i] = BitConverter.ToInt32(buffer);
            }

            return result;
        }

        #endregion
    }
}
