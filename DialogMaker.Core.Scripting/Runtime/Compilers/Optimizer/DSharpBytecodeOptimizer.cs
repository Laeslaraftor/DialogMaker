using DialogMaker.Core.Scripting.Runtime.Builders;
using static DialogMaker.Core.Scripting.Runtime.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public static partial class DSharpBytecodeOptimizer
    {
        public static void Optimize(DSharpAssemblyBuilder assembly)
        {
            HashSet<DSharpTypeBuilder> optimizedTypes = [];

            void OptimizeType(DSharpTypeBuilder type)
            {
                if (type.ObjectType == DSharpObjectType.Enum ||
                    !optimizedTypes.Add(type))
                {
                    return;
                }
                if (type.GenericTemplate != null)
                {
                    if (type.GenericTemplate is DSharpTypeBuilder genericTemplateBuilder)
                    {
                        OptimizeType(genericTemplateBuilder);
                    }

                    return;
                }

                foreach (var method in GetAllMethods(type))
                {
                    Optimize(method);
                }
            }

            foreach (var type in assembly.Types)
            {
                OptimizeType(type);
            }
            foreach (var globalFunction in assembly.GlobalFunctions)
            {
                Optimize(globalFunction);
            }
        }
        public static void Optimize(DSharpMethodBuilder method)
        {
            if (method.IsAbstract || method.IsExtern)
            {
                return;
            }

            var code = method.GetBytecodeBuilder();
            OptimizePops(code);
            OptimizeUselessCombinations(code);
        }

        #region Дополнительно

        private static IEnumerable<InstructionRange> FindRepeatInstructions(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation)
        {
            return FindRepeatInstructions<Instruction>(builder, operation, null);
        }
        private static IEnumerable<InstructionRange> FindRepeatInstructions<T>(DSharpBytecodeBuilder builder, DSharpBytecodeOperation operation, Func<T, T?, bool>? predicate)
            where T : Instruction
        {
            int lastRangeStart = -1;
            int currentRangeCount = 0;
            T? previousInstruction = null;
            int i = 0;

            bool Check(T instruction)
            {
                if (predicate == null)
                {
                    return true;
                }

                return predicate(instruction, previousInstruction);
            }

            foreach (var instruction in builder.Instructions)
            {
                if (instruction.Operation == operation && instruction is T typedInstruction && Check(typedInstruction))
                {
                    if (lastRangeStart == -1)
                    {
                        lastRangeStart = i;
                        currentRangeCount = 1;
                    }
                    else
                    {
                        currentRangeCount++;
                    }

                    previousInstruction = typedInstruction;
                }
                else if (lastRangeStart > -1)
                {
                    if (currentRangeCount > 1)
                    {
                        yield return new(lastRangeStart, currentRangeCount);
                    }

                    lastRangeStart = -1;
                    currentRangeCount = 0;
                    previousInstruction = null;
                }

                i++;
            }
        }

        private static IEnumerable<DSharpMethodBuilder> GetAllMethods(DSharpTypeBuilder type)
        {
            foreach (var method in type.Methods)
            {
                yield return method;
            }
            foreach (var constructor in type.Constructors)
            {
                yield return constructor;
            }
        }

        #endregion

        #region Структуры

        private readonly struct InstructionRange(int startIndex, int length)
        {
            public int StartIndex { get; } = startIndex;
            public int EndIndex { get; } = startIndex + length;
            public int Length { get; } = length;
        }

        #endregion
    }
}
