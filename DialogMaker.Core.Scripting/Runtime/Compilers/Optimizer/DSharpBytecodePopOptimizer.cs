using DialogMaker.Core.Scripting.Runtime.Builders;
using static DialogMaker.Core.Scripting.Runtime.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public static partial class DSharpBytecodeOptimizer
    {
        private static void OptimizePops(DSharpBytecodeBuilder builder)
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
    }
}
