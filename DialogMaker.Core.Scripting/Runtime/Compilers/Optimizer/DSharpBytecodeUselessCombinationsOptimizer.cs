using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Collections.ObjectModel;
using static DialogMaker.Core.Scripting.Runtime.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public static partial class DSharpBytecodeOptimizer
    {
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _storeParameterOperations = new([
            DSharpBytecodeOperation.StoreArgument,
            DSharpBytecodeOperation.StoreLocal
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _loadParameterOperations = new([
            DSharpBytecodeOperation.LoadArgument,
            DSharpBytecodeOperation.LoadLocal
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _storeMemberOperations = new([
            DSharpBytecodeOperation.StoreField,
            DSharpBytecodeOperation.StoreInstanceField,
            DSharpBytecodeOperation.StoreProperty,
            DSharpBytecodeOperation.StoreInstanceProperty
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _loadMemberOperations = new([
            DSharpBytecodeOperation.LoadField,
            DSharpBytecodeOperation.LoadInstanceField,
            DSharpBytecodeOperation.LoadProperty,
            DSharpBytecodeOperation.LoadInstanceProperty
        ]);
        private static readonly Range _uselessPopCombinationsRange = new(0, 5);
        private static readonly ReadOnlyCollection<UselessCombination> _uselessCombinations =
        new([
            new(
                [
                    new(_storeParameterOperations, typeof(ParameterInstruction), 2),
                    new(DSharpBytecodeOperation.Pop),
                    new(_loadParameterOperations, typeof(ParameterInstruction)),
                ],
                UselessCombinationRemoveNextTwo
            ),
            new(
                [
                    new(_storeMemberOperations, typeof(TypeInstruction), 2),
                    new(DSharpBytecodeOperation.Pop),
                    new(_loadMemberOperations, typeof(TypeInstruction)),
                ],
                UselessCombinationRemoveNextTwo
            ),
            new(
                [
                    new(DSharpBytecodeOperation.PopOffset, typeof(IndexInstruction)),
                    new(DSharpBytecodeOperation.Return),
                ],
                UselessCombinationRemoveFirst
            ),
            new(
                [
                    new(DSharpBytecodeOperation.PopOffsetRepeat, typeof(OffsetCountInstruction)),
                    new(DSharpBytecodeOperation.Return),
                ],
                UselessCombinationRemoveFirst
            ),
            new(
                [
                    new(DSharpBytecodeOperation.Empty),
                    InstructionDefinition.Any,
                ],
                UselessCombinationRemoveEmpty
            ),
            new(
                [
                    new(_storeParameterOperations, typeof(ParameterInstruction)),
                    new(DSharpBytecodeOperation.Pop),
                    new(DSharpBytecodeOperation.Jump, new InstructionDefinition(_loadParameterOperations, typeof(ParameterInstruction), 0)),
                ],
                UselessCombinationRemovePopBeforeJump
            ),
            new(
                [
                    new(_storeMemberOperations, typeof(ParameterInstruction)),
                    new(DSharpBytecodeOperation.Pop),
                    new(DSharpBytecodeOperation.Jump, new InstructionDefinition(_loadMemberOperations, typeof(TypeInstruction), 0)),
                ],
                UselessCombinationRemovePopBeforeJump
            ),
            new(
                [
                    new(DSharpBytecodeOperation.Push, typeof(LiteralInstruction), 3),
                    new(_storeParameterOperations, typeof(ParameterInstruction)),
                    new(DSharpBytecodeOperation.Pop),
                    new(DSharpBytecodeOperation.Push, typeof(LiteralInstruction))
                ],
                UselessCombinationRemoveSamePush
            ),
            new(
                [
                    new(DSharpBytecodeOperation.Push, typeof(LiteralInstruction), 3),
                    new(_storeMemberOperations, typeof(ParameterInstruction)),
                    new(DSharpBytecodeOperation.Pop),
                    new(DSharpBytecodeOperation.Push, typeof(LiteralInstruction))
                ],
                UselessCombinationRemoveSamePush
            ),
            new(
                [
                    new(DSharpBytecodeOperation.PopOffsetRepeat, typeof(OffsetCountInstruction), 1, 2)
                ],
                UselessCombinationRemovePopOffset12
            ),
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _finalUselessOperations =
        new([
            DSharpBytecodeOperation.Pop,
            DSharpBytecodeOperation.PopOffset,
            DSharpBytecodeOperation.PopOffsetRepeat,
            DSharpBytecodeOperation.PopPreviousTwo,
            DSharpBytecodeOperation.PopRepeat
        ]);
        private static bool _uselessCombinationOptimizationCheckReferences = true;

        private static int OptimizeUselessCombinations(DSharpBytecodeBuilder builder)
        {
            return OptimizeUselessCombinations(builder, new(0, _uselessCombinations.Count - 1), 0, int.MaxValue);
        }
        private static int OptimizeUselessCombinations(DSharpBytecodeBuilder builder, Range uselessCombinationsRange, int startInstruction, int maxCombinationsCount)
        {
            var instructions = builder.Instructions;
            Dictionary<int, UselessCombination> startIndexOfUselessCombination = [];
            int offset = 0;
            int maxLength = Math.Min(_uselessCombinations.Count, uselessCombinationsRange.Start.Value + uselessCombinationsRange.End.Value + 1);

            for (int i = startInstruction; i < builder.Instructions.Count; i++)
            {
                for (int c = uselessCombinationsRange.Start.Value; c < maxLength; c++)
                {
                    var uselessCombination = _uselessCombinations[c];

                    if (uselessCombination.SequenceEquals(instructions, i))
                    {
                        startIndexOfUselessCombination.Add(i, uselessCombination);
                        break;
                    }
                }

                if (startIndexOfUselessCombination.Count >= maxCombinationsCount)
                {
                    break;
                }
            }
            foreach (var info in startIndexOfUselessCombination)
            {
                var startIndex = info.Key - offset;
                offset += info.Value.Remove(builder, startIndex);
            }

            if (maxCombinationsCount == int.MaxValue)
            {
                while (instructions.Count > 0 &&
                       _finalUselessOperations.Contains(instructions[^1].Operation))
                {
                    ReplaceInstructionReferences(builder, instructions.Count - 1);
                    offset++;
                }
            }

            return offset;
        }

        #region Дополнительно

        private static int UselessCombinationRemovePopOffset12(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            var currentInstruction = builder.Instructions[startIndex];
            Instruction newInstruction = new(builder, DSharpBytecodeOperation.PopPreviousTwo);
            builder.Instructions[startIndex] = newInstruction;

            ReplaceInstructionReferences(builder, currentInstruction, newInstruction);

            return 0;
        }
        private static int UselessCombinationRemoveSamePush(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            var instructions = builder.Instructions;
            int lastPushIndex = startIndex + uselessCombination.Definitions.Count - 1;
            var lastPush = instructions[lastPushIndex];

            if (_uselessCombinationOptimizationCheckReferences)
            {
                var references = builder.FindReferences(lastPush);

                if (references != null && references.Count > 0)
                {
                    return 0;
                }
            }

            startIndex += 2;

            for (int i = 0; i < 2; i++)
            {
                ReplaceInstructionReferences(builder, startIndex);
            }

            return 2;
        }
        private static int UselessCombinationRemovePopBeforeJump(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            if (uselessCombination.Definitions.Count == 0)
            {
                return 0;
            }

            var lastDefinition = uselessCombination.Definitions[^1];

            if (lastDefinition.ReferencesInstruction == null)
            {
                return 0;
            }

            var indexOfReferenceInstruction = startIndex + uselessCombination.Definitions.Count - 1;
            var instructions = builder.Instructions;
            var popInstruction = instructions[startIndex + 1];

            if (instructions[indexOfReferenceInstruction] is not ReferenceInstruction referenceInstruction ||
                referenceInstruction.ReferencedInstruction == null)
            {
                return 0;
            }

            int referenceIndex = instructions.IndexOf(referenceInstruction.ReferencedInstruction) - 2;

            if (0 > referenceIndex)
            {
                return 0;
            }

            bool startCheckReferencesValue = _uselessCombinationOptimizationCheckReferences;
            _uselessCombinationOptimizationCheckReferences = false;
            int offset = OptimizeUselessCombinations(builder, _uselessPopCombinationsRange, referenceIndex, 1);
            _uselessCombinationOptimizationCheckReferences = startCheckReferencesValue;

            if (offset == 0)
            {
                return 0;
            }

            ReplaceInstructionReferences(builder, instructions.IndexOf(popInstruction));

            return 1 + offset;
        }
        private static int UselessCombinationRemoveEmpty(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            if (startIndex + 1 >= builder.Instructions.Count)
            {
                return 0;
            }

            return UselessCombinationRemoveFirst(uselessCombination, builder, startIndex);
        }
        private static int UselessCombinationRemoveFirst(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            ReplaceInstructionReferences(builder, startIndex);
            return 1;
        }
        private static int UselessCombinationRemoveNextTwo(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            return UselessCombinationRemoveNextTwo(uselessCombination, builder, startIndex, _uselessCombinationOptimizationCheckReferences);
        }
        private static int UselessCombinationRemoveNextTwo(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex, bool checkReferences)
        {
            var instructions = builder.Instructions;
            startIndex++;

            if (checkReferences)
            {
                for (int i = startIndex; i < startIndex + 2; i++)
                {
                    var instruction = instructions[i];
                    var references = builder.FindReferences(instruction);

                    if (references != null && references.Count > 0)
                    {
                        return 0;
                    }
                }
            }
            for (int i = startIndex; i < startIndex + 2; i++)
            {
                if (checkReferences)
                {
                    instructions.RemoveAt(startIndex);
                }
                else
                {
                    ReplaceInstructionReferences(builder, startIndex);
                }
            }

            return 2;
        }

        private static void ReplaceInstructionReferences(DSharpBytecodeBuilder builder, int instructionIndex)
        {
            var oldInstruction = builder.Instructions[instructionIndex];
            builder.Instructions.RemoveAt(instructionIndex);

            Instruction newInstruction;

            if (instructionIndex >= builder.Instructions.Count)
            {
                newInstruction = builder.Instructions[^1];
            }
            else
            {
                newInstruction = builder.Instructions[instructionIndex];
            }

            ReplaceInstructionReferences(builder, oldInstruction, newInstruction);
        }
        private static void ReplaceInstructionReferences(DSharpBytecodeBuilder builder, Instruction original, Instruction newReference)
        {
            var references = builder.FindReferences(original);

            if (references != null)
            {
                foreach (var reference in references)
                {
                    reference.ReferencedInstruction = newReference;
                }
            }
        }

        #endregion

        #region Структуры

        private readonly struct UselessCombination(InstructionDefinition[] definitions, Func<UselessCombination, DSharpBytecodeBuilder, int, int> remover)
        {
            public ReadOnlyCollection<InstructionDefinition> Definitions { get; } = new(definitions);

            private readonly Func<UselessCombination, DSharpBytecodeBuilder, int, int> _remover = remover;

            public int Remove(DSharpBytecodeBuilder builder, int startIndex)
            {
                return _remover(this, builder, startIndex);
            }
            public bool SequenceEquals(IList<Instruction> instructions, int startIndex)
            {
                if (startIndex + Definitions.Count > instructions.Count)
                {
                    return false;
                }

                object[][] instructionArguments = new object[Definitions.Count][];
                Dictionary<InstructionDefinition, object[]> referenceArguments = [];

                for (int i = 0; i < Definitions.Count; i++)
                {
                    var definition = Definitions[i];
                    var instruction = instructions[i + startIndex];

                    if (!definition.Equals(instruction))
                    {
                        return false;
                    }

                    instructionArguments[i] = instruction.GetArguments();

                    if (instruction is ReferenceInstruction referenceInstruction)
                    {
                        var reference = referenceInstruction.ReferencedInstruction;
                        object[] args = [];

                        if (reference != null)
                        {
                            args = reference.GetArguments();
                        }

                        referenceArguments.TryAdd(definition, args);
                    }
                }
                for (int i = 0; i < Definitions.Count; i++)
                {
                    var definition = Definitions[i];
                    var args = instructionArguments[i];

                    if (definition.SameArgsTo != -1 &&
                        !args.SequenceEqual(instructionArguments[definition.SameArgsTo]))
                    {
                        return false;
                    }
                    else if (definition.SameArgsTo == -1 &&
                             definition.ExactArguments != null &&
                             !args.SequenceEqual(definition.ExactArguments))
                    {
                        return false;
                    }
                    else if (definition.ReferencesInstruction != null &&
                        definition.ReferencesInstruction.SameArgsTo != -1 &&
                        (!referenceArguments.TryGetValue(definition, out var referenceArgs) ||
                        !referenceArgs.SequenceEqual(instructionArguments[definition.ReferencesInstruction.SameArgsTo])))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        private class InstructionDefinition(IEnumerable<DSharpBytecodeOperation>? operations, Type? instructionType, int sameArgsTo = -1, InstructionDefinition? referencesInstruction = null)
            : IEquatable<Instruction>
        {
            public InstructionDefinition(DSharpBytecodeOperation operation, Type? instructionType, int sameArgsTo = -1, InstructionDefinition? referencesInstruction = null)
                : this([operation], instructionType, sameArgsTo, referencesInstruction)
            {
            }
            public InstructionDefinition(DSharpBytecodeOperation operation)
                : this([operation], typeof(Instruction))
            {
            }
            public InstructionDefinition(DSharpBytecodeOperation operation, Type instructionType, params object[] exactArguments)
                : this([operation], instructionType)
            {
                ExactArguments = new(exactArguments);
            }
            public InstructionDefinition(DSharpBytecodeOperation operation, InstructionDefinition reference)
                : this([operation], typeof(ReferenceInstruction), referencesInstruction: reference)
            {
            }

            public ReadOnlyCollection<DSharpBytecodeOperation> Operations { get; } = new(operations == null ? [] : [.. operations]);
            public Type? InstructionType { get; } = instructionType;
            public int SameArgsTo { get; } = sameArgsTo;
            public ReadOnlyCollection<object>? ExactArguments { get; }
            public InstructionDefinition? ReferencesInstruction { get; } = referencesInstruction;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="other"><inheritdoc/></param>
            /// <returns><inheritdoc/></returns>
            public bool Equals(Instruction other)
            {
                if (Operations.Count == 0 && InstructionType == null)
                {
                    return true;
                }

                bool result = Operations.Contains(other.Operation) &&
                              other.GetType() == InstructionType;

                if (ReferencesInstruction != null)
                {
                    if (other is not ReferenceInstruction otherReference ||
                        otherReference.ReferencedInstruction == null)
                    {
                        return false;
                    }

                    return result && ReferencesInstruction.Equals(otherReference.ReferencedInstruction);
                }

                return result;
            }

            public static readonly InstructionDefinition Any = new(null, null);
        }

        #endregion
    }
}
