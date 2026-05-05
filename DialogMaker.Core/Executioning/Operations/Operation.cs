using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning
{
    public struct Operation : IEquatable<Operation>
    {
        public Operation(DialogByteCode code)
        {
            Code = code;
            Arguments = CreateArguments<int>(code);
        }

        public DialogByteCode Code { get; }
        public int[] Arguments { get; }
        public bool IsValid
        {
            get
            {
                foreach (var arg in Arguments)
                {
                    if (0 > arg)
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        #region Управление

        public readonly bool Equals(Operation other)
        {
            if (Code != other.Code || Arguments?.Length != other.Arguments?.Length)
            {
                return false;
            }
            if (Arguments != null && other.Arguments != null)
            {
                for (int i = 0; i < Arguments.Length; i++)
                {
                    if (Arguments[i] != other.Arguments[i])
                    {
                        return false;
                    }
                }
            }

            return (Arguments == null && other.Arguments == null) ||
                   (Arguments != null && other.Arguments != null);
        }
        public readonly override bool Equals(object obj)
        {
            return obj is Operation other &&
                   Equals(other);
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Code, Arguments);
        }
        public readonly override string ToString()
        {
            if (Arguments.Length == 0)
            {
                return Code.ToString();
            }

            string args = string.Empty;

            foreach (var arg in Arguments)
            {
                if (args != string.Empty)
                {
                    args += ", ";
                }

                args += arg;
            }

            return $"{Code}({args})";
        }

        #endregion

        #region Операторы

        public static bool operator ==(Operation o1, Operation o2) => o1.Equals(o2);
        public static bool operator !=(Operation o1, Operation o2) => !o1.Equals(o2);

        #endregion

        #region Статика

        public const int ArgumentSize = sizeof(int);
        public const int InstructionSize = sizeof(DialogByteCode);

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

        public static T[] CreateArguments<T>(DialogByteCode code)
        {
            int length = CodeArguments[code];

            return length > 0 ? new T[length] : [];
        }
        public static OpCode GetImplementation(DialogByteCode code)
        {
            if (!_opCodeInstances.TryGetValue(code, out var instance))
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

            if (result.Arguments.Length == 0)
            {
                return result;
            }

            Span<byte> buffer = stackalloc byte[ArgumentSize];

            for (int i = 0; i < result.Arguments.Length; i++)
            {
                int bytesRead = 0;

                for (int v = 0; v < buffer.Length; v++)
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
