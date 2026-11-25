using System.Collections.Generic;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    internal static class CodeGeneratorHelper
    {
        public static readonly Dictionary<string, string> GeneratedTypes = [];

        public const string Tab = "    ";
        public const string ImmutableArrayFullName = "global::System.Collections.Immutable.ImmutableArray";
        public const string ReadOnlyDictionaryFullName = "global::System.Collections.ObjectModel.ReadOnlyDictionary";
        public const string DictionaryFullName = "global::System.Collections.Generic.Dictionary";
        public const string OperationEnumFullName = "global::DialogMaker.Core.Scripting.Runtime.DSharpBytecodeOperation";
        public const string ExecutorAttributeFullName = "global::DialogMaker.Core.Scripting.Runtime.ExecutorAttribute";
        public const string InfoStructFullName = "global::DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.DSharpOperationExecutorInfo";
        public const string NameAttributeFullName = "global::DialogMaker.Core.Scripting.CodeAnalyzer.NameAttribute";
        public const string IgnoreAttributeFullName = "global::DialogMaker.Core.Scripting.CodeAnalyzer.IgnoreAttribute";

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
