using DialogMaker.Core.Scripting.Runtime.Builders;
using static DialogMaker.Core.Scripting.Runtime.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public static class DSharpBytecodeOptimizer
    {
        public static void Optimize(DSharpMethodBuilder method)
        {
            if (method.IsAbstract || method.IsExtern)
            {
                return;
            }

            var code = method.GetBytecodeBuilder();
            Optimize(code);
        }
        public static void Optimize(DSharpBytecodeBuilder builder)
        {
            var popRepeats = FindRepeatInstructions(builder, DSharpBytecodeOperation.Pop).ToList();
            popRepeats.Reverse();

            foreach (var popRange in popRepeats)
            {
                for (int i = 0; i < popRange.Length; i++)
                {
                    builder.Instructions.RemoveAt(popRange.StartIndex);
                }

                IndexInstruction newInstruction = new(builder, DSharpBytecodeOperation.PopRepeat, popRange.Length);
                builder.Instructions.Insert(popRange.StartIndex, newInstruction);
            }
            
            static bool PredicateIndexInstruction(IndexInstruction current, IndexInstruction? previous)
            {
                if (previous == null)
                {
                    return true;
                }

                return current.Index == previous.Index;
            }

            var popOffsetRepeats = FindRepeatInstructions<IndexInstruction>(builder, DSharpBytecodeOperation.PopOffset, PredicateIndexInstruction).ToList();
            popOffsetRepeats.Reverse();

            foreach (var popRange in popOffsetRepeats)
            {
                if (builder.Instructions[popRange.StartIndex] is not IndexInstruction indexInstruction)
                {
                    continue;
                }

                int index = indexInstruction.Index;

                for (int i = 0; i < popRange.Length; i++)
                {
                    builder.Instructions.RemoveAt(popRange.StartIndex);
                }

                OffsetCountInstruction newInstruction = new(builder, DSharpBytecodeOperation.PopOffsetRepeat, index, popRange.Length);
                builder.Instructions.Insert(popRange.StartIndex, newInstruction);
            }
        }

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

        private struct InstructionRange(int startIndex, int length)
        {
            public int StartIndex { get; } = startIndex;
            public int EndIndex { get; } = startIndex + length;
            public int Length { get; } = length;
        }
    }
}
