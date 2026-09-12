namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    internal static class CodeGeneratorHelper
    {
        public const string Tab = "    ";
        public const string ImmutableArrayFullName = "global::System.Collections.Immutable.ImmutableArray";
        public const string ReadOnlyDictionaryFullName = "global::System.Collections.ObjectModel.ReadOnlyDictionary";
        public const string DictionaryFullName = "global::System.Collections.Generic.Dictionary";

        public static string GetIndent(int indent)
        {
            string result = string.Empty;

            for (int i = 0; i < indent; i++)
            {
                result += Tab;
            }

            return result;
        }
    }
}
