using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting
{
    internal static class DictionaryExtensions
    {
        extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            public bool TryGetKey(TValue value, [NotNullWhen(true)] out TKey? result)
            {
                result = dictionary.FirstOrDefault(p => Equals(p.Value, value)).Key;
                return !Equals(result, default);
            }
        }
    }
}
