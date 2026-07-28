namespace System;

public struct Int64
{
    public override string ToString() => GetString(this);

    internal static string GetString(long number)
    {
        // Обработка нуля
        if (number == 0)
        {
            return "0";
        }

        bool isNegative = number < 0;
        // Берем модуль числа, чтобы работать с положительным числом
        int absValue = Math.Abs(number);

        // Временный массив для хранения цифр (максимум 10 цифр для int)
        char[] tempChars = new char[11]; // 10 цифр + 1 знак минуса
        int index = 0;

        // Пока число больше 0, извлекаем последнюю цифру
        while (absValue > 0)
        {
            int remainder = absValue % 10;          // Берем остаток от деления на 10
            tempChars[index] = (char)('0' + remainder); // Превращаем цифру в символ
            absValue /= 10;                         // Убираем последнюю цифру
            index++;
        }

        // Если число отрицательное, добавляем минус в конец массива (но потом развернем)
        if (isNegative)
        {
            tempChars[index] = '-';
            index++;
        }

        // Разворачиваем массив, так как мы записывали цифры справа налево
        char[] result = new char[index];
        for (int i = 0; i < index; i++)
        {
            result[i] = tempChars[index - 1 - i];
        }

        return new string(result);
    }
}