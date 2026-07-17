using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using static DialogMaker.Core.Scripting.Compiler.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Scripting.Compiler
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

                IndexInstruction newInstruction = new(builder, DSharpBytecodeOperation.PopRepeat, (uint)popRange.Length);
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

                int index = (int)indexInstruction.Index;

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
