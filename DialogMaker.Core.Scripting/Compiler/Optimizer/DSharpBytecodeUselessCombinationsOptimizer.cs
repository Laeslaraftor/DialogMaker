using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using static DialogMaker.Core.Scripting.Compiler.Builders.DSharpBytecodeBuilder;

namespace DialogMaker.Core.Scripting.Compiler
{
    public static partial class DSharpBytecodeOptimizer
    {
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _storeParameterOperations = new([
            DSharpBytecodeOperation.StoreLocal
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _loadParameterOperations = new([
            DSharpBytecodeOperation.LoadLocal
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _storeMemberOperations = new([
            DSharpBytecodeOperation.StoreField,
            DSharpBytecodeOperation.StoreInstanceField,
            DSharpBytecodeOperation.StoreProperty,
            DSharpBytecodeOperation.StoreIndexer
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _loadMemberOperations = new([
            DSharpBytecodeOperation.LoadField,
            DSharpBytecodeOperation.LoadInstanceField,
            DSharpBytecodeOperation.LoadProperty,
            DSharpBytecodeOperation.LoadIndexer
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _potentialLoadMemberOperations = new([.. _loadMemberOperations.Append(DSharpBytecodeOperation.Call)]);
        private static readonly Range _uselessPopCombinationsRange = new(0, 5);
        private static readonly ReadOnlyCollection<UselessCombination> _uselessCombinations = new([
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
            new(
                [
                    new(DSharpBytecodeOperation.StartScope),
                    new(DSharpBytecodeOperation.Jump, typeof(ReferenceInstruction)),
                    new(DSharpBytecodeOperation.EndScope),
                ],
                UselessCombinationRemoveJumpScope
            ),
            new(
                [
                    new(DSharpBytecodeOperation.GetAddress, true),
                    new(DSharpBytecodeOperation.New, true),
                    new(DSharpBytecodeOperation.PopOffset, typeof(IndexInstruction), exactArguments: 1u),
                    new(DSharpBytecodeOperation.LoadInstanceField, true),
                    new(DSharpBytecodeOperation.New, true),
                    new(DSharpBytecodeOperation.PopPreviousTwo),
                    new(DSharpBytecodeOperation.LoadInstanceField, true),
                    new(DSharpBytecodeOperation.ReadOnAddress, true),
                    new(DSharpBytecodeOperation.PopPreviousTwo)
                ],
                UselessCombinationRemovePointerConverting
            ),
            new(
                [
                    InstructionDefinition.Any,
                    new(DSharpBytecodeOperation.Call, typeof(CallingInstruction)),
                    new(DSharpBytecodeOperation.PopOffset, typeof(IndexInstruction), exactArguments: 1u),
                    new(DSharpBytecodeOperation.LoadInstanceField, typeof(TypeInstruction)),
                    new(DSharpBytecodeOperation.ReadOnAddress, typeof(TypeInstruction))
                ],
                UselessCombinationRemovePointerCreatingBeforeReading
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
                    new(DSharpBytecodeOperation.PopPreviousTwo),
                    new(DSharpBytecodeOperation.Return),
                ],
                UselessCombinationRemoveFirst
            )
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _finalUselessOperations = new([
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

            if (maxCombinationsCount == int.MaxValue && instructions.Count > 0)
            {
                Dictionary<Instruction, List<ReferenceInstruction>> referencesToUselessOperations = [];
                int currentIndex = instructions.Count - 1;

                while (currentIndex > 0 &&
                       _finalUselessOperations.Contains(instructions[currentIndex].Operation))
                {
                    var currentInstruction = instructions[currentIndex];
                    var references = builder.FindReferences(currentInstruction);

                    if (references != null)
                    {
                        referencesToUselessOperations.Add(currentInstruction, references);
                    }

                    instructions.RemoveAt(currentIndex);
                    currentIndex--;
                }

                if (referencesToUselessOperations.Count > 0)
                {
                    var empty = builder.Empty();

                    foreach (var references in referencesToUselessOperations.Values)
                    {
                        foreach (var reference in references)
                        {
                            reference.ReferencedInstruction = empty;
                        }
                    }
                }
            }

            return offset;
        }

        #region Optimizations

        private static int UselessCombinationRemovePointerCreatingBeforeReading(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            var loadAddressInstruction = builder.Instructions[startIndex];
            IDSharpType loadedAddressType;

            if (loadAddressInstruction is ParameterInstruction parameterInstruction)
            {
                loadedAddressType = parameterInstruction.Parameter.Type;
            }
            else if (loadAddressInstruction is CallingInstruction callingInstruction)
            {
                if (callingInstruction.AccessedMember.TryGetReturnType(out var returnType))
                {
                    loadedAddressType = returnType;
                }

                return 0;
            }
            else if (loadAddressInstruction is TypeInstruction typeInstruction &&
                     typeInstruction.MemberInfo is IDSharpFieldInfo fieldInfo)
            {
                loadedAddressType = fieldInfo.FieldType;
            }
            else
            {
                return 0;
            }
            if (loadedAddressType != builder.Method.Assembly.NIntType ||
                builder.Instructions[startIndex + 1] is not CallingInstruction creatingPointerInstruction ||
                creatingPointerInstruction.AccessedMember is not IDSharpMethodInfo method ||
                method.ReturnType?.GenericTemplate != builder.Method.Assembly.TypedPointerType)
            {
                return 0;
            }

            var pointerTypeInfo = DSharpPointerType.Create(method.ReturnType);

            if (builder.Instructions[startIndex + 3] is not TypeInstruction loadAddressFieldInstruction ||
                loadAddressFieldInstruction.MemberInfo != pointerTypeInfo.AddressField ||
                builder.Instructions[startIndex + 4] is not TypeInstruction readAddressInstruction ||
                readAddressInstruction.MemberInfo != pointerTypeInfo.ValueType)
            {
                return 0;
            }

            builder.Instructions.RemoveRange(startIndex + 1, 3);

            return 3;
        }
        private static int UselessCombinationRemovePointerConverting(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            for (int i = 0; i < 6; i++)
            {
                builder.Instructions.RemoveAt(startIndex + 1);
            }


            if (builder.Instructions.Count > startIndex + 2 &&
                builder.Instructions[startIndex + 3].Operation == DSharpBytecodeOperation.Return)
            {
                ReplaceInstructionReferences(builder, startIndex + 2);
                return 7;
            }

            var instructionToRemove = builder.Instructions[startIndex + 2];
            IndexInstruction popOffset = new(builder, DSharpBytecodeOperation.PopOffset, 1);
            builder.Instructions[startIndex + 2] = popOffset;

            ReplaceInstructionReferences(builder, instructionToRemove, popOffset);

            return 6;
        }
        private static int UselessCombinationRemoveJumpScope(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            ReplaceInstructionReferences(builder, startIndex);
            ReplaceInstructionReferences(builder, startIndex + 1);

            return 2;
        }
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

            int offset = 0;

            if (_uselessCombinationOptimizationCheckReferences)
            {
                bool startCheckReferencesValue = _uselessCombinationOptimizationCheckReferences;
                _uselessCombinationOptimizationCheckReferences = false;
                offset = OptimizeUselessCombinations(builder, _uselessPopCombinationsRange, referenceIndex, 1);
                _uselessCombinationOptimizationCheckReferences = startCheckReferencesValue;

                if (offset == 0)
                {
                    return 0;
                }
            }

            int popInstructionIndex = instructions.IndexOf(popInstruction);

            if (popInstructionIndex == -1)
            {
                return offset;
            }

            ReplaceInstructionReferences(builder, popInstructionIndex);

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

        #region Structs

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
        private class InstructionDefinition(IEnumerable<DSharpBytecodeOperation>? operations, Type? instructionType, int sameArgsTo = -1, InstructionDefinition? referencesInstruction = null, bool anyInstructionType = false)
            : IEquatable<Instruction>
        {
            public InstructionDefinition(DSharpBytecodeOperation operation, Type? instructionType, int sameArgsTo = -1, InstructionDefinition? referencesInstruction = null)
                : this([operation], instructionType, sameArgsTo, referencesInstruction)
            {
            }
            public InstructionDefinition(DSharpBytecodeOperation operation, bool anyInstructionType = false)
                : this([operation], typeof(Instruction), anyInstructionType: anyInstructionType)
            {
            }
            public InstructionDefinition(IEnumerable<DSharpBytecodeOperation>? operations, bool anyInstructionType = false)
                : this(operations, typeof(Instruction), anyInstructionType: anyInstructionType)
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
            public bool AnyInstructionType { get; } = anyInstructionType;

            public bool Equals(Instruction? other)
            {
                if (other == null || Operations.Count == 0 && InstructionType == null)
                {
                    return true;
                }

                bool result = Operations.Contains(other.Operation) &&
                              (AnyInstructionType || other.GetType() == InstructionType);

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
