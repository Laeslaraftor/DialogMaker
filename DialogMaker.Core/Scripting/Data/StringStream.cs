namespace DialogMaker.Core.Scripting.Data
{
    /// <summary>
    /// Поток строки
    /// </summary>
    /// <param name="value">Строковое значение</param>
    public class StringStream(string value)
    {
        /// <summary>
        /// Читаемая строка
        /// </summary>
        public string Value { get; } = value;
        /// <summary>
        /// Текущая позиция в строке
        /// </summary>
        public int Position
        {
            get;
            set => field = Math.Max(0, Math.Min(Value.Length, value));
        }
        /// <summary>
        /// Текущее значение
        /// </summary>
        public char CurrentValue => Value[Position];

        #region Управление

        /// <summary>
        /// Прочитать следующий символ. Текущая позиция будет увеличена на 1
        /// </summary>
        /// <returns>Следующий символ. Если достигнут конец строки, то будет возвращено \0</returns>
        public char ReadChar()
        {
            var position = Position;

            if (position < Value.Length)
            {
                var result = Value[position];
                Position++;
                return result;
            }

            return '\0';
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <returns>Прочтённая строка</returns>
        public string ReadWhile(Predicate<char> predicate)
        {
            return ReadWhile(predicate, true);
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <param name="include">Включить ли последний символ в строку</param>
        /// <returns>Прочтённая строка</returns>
        public string ReadWhile(Predicate<char> predicate, bool include)
        {
            ReadWhile((s, c) => predicate(c), include, out var result);
            return result;
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <param name="result">Прочтённая строка</param>
        public void ReadWhile(Predicate<char> predicate, out string result)
        {
            ReadWhile(predicate, true, out result);
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <param name="include">Включить ли последний символ в строку</param>
        /// <param name="result">Прочтённая строка</param>
        public void ReadWhile(Predicate<char> predicate, bool include, out string result)
        {
            ReadWhile((s, c) => predicate(c), include, out result);
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <returns>Прочтённая строка</returns>
        public string ReadWhile(Func<string, char, bool> predicate)
        {
            return ReadWhile(predicate, true);
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <param name="include">Включить ли последний символ в строку</param>
        /// <returns>Прочтённая строка</returns>
        public string ReadWhile(Func<string, char, bool> predicate, bool include)
        {
            ReadWhile(predicate, include, out var result);
            return result;
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <param name="result">Прочтённая строка</param>
        public void ReadWhile(Func<string, char, bool> predicate, out string result)
        {
            ReadWhile(predicate, true, out result);
        }
        /// <summary>
        /// Прочитать строку, начиная с текущей позиции, пока условие верно
        /// </summary>
        /// <param name="predicate">Условие чтения строки</param>
        /// <param name="include">Включить ли последний символ в строку</param>
        /// <param name="result">Прочтённая строка</param>
        public void ReadWhile(Func<string, char, bool> predicate, bool include, out string result)
        {
            result = string.Empty;
            char value;
            bool predicateResult;

            do
            {
                value = ReadChar();
                predicateResult = predicate(result, value);

                if (!predicateResult)
                {
                    if (include)
                    {
                        result += value;
                    }
                    else
                    {
                        if (value != '\0')
                        {
                            Position--;
                        }
                    }

                    break;
                }

                result += value;
            }
            while (value != '\0');
        }

        #endregion
    }
}
