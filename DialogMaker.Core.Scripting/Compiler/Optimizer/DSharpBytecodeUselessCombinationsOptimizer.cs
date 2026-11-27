using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
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
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _storeOperations = new([
            DSharpBytecodeOperation.StoreLocal,
            DSharpBytecodeOperation.StoreField,
            DSharpBytecodeOperation.StoreInstanceField,
            DSharpBytecodeOperation.StoreProperty,
            DSharpBytecodeOperation.StoreIndexer
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _loadOperations = new([
            DSharpBytecodeOperation.LoadLocal,
            DSharpBytecodeOperation.LoadField,
            DSharpBytecodeOperation.LoadInstanceField,
            DSharpBytecodeOperation.LoadProperty,
            DSharpBytecodeOperation.LoadIndexer
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _conditionalJumpOperations = new([
            DSharpBytecodeOperation.JumpIfFalse,
            DSharpBytecodeOperation.JumpIfTrue
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _nativeStoreOperations = new([
            DSharpBytecodeOperation.StoreLocal,
            DSharpBytecodeOperation.StoreField,
            DSharpBytecodeOperation.StoreInstanceField
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _assignableBinaryOperations = new([
            DSharpBytecodeOperation.AddVariables,
            DSharpBytecodeOperation.AddVariableField,
            DSharpBytecodeOperation.SubtractVariables,
            DSharpBytecodeOperation.SubtractVariableField,
            DSharpBytecodeOperation.SubtractFieldVariable,
            DSharpBytecodeOperation.SubtractFields,
            DSharpBytecodeOperation.MultiplyVariables,
            DSharpBytecodeOperation.MultiplyVariableField,
            DSharpBytecodeOperation.MultiplyFields,
            DSharpBytecodeOperation.DivideVariables,
            DSharpBytecodeOperation.DivideVariableField,
            DSharpBytecodeOperation.DivideFieldVariable,
            DSharpBytecodeOperation.DivideFields,
            DSharpBytecodeOperation.ModVariables,
            DSharpBytecodeOperation.ModVariableField,
            DSharpBytecodeOperation.ModFieldVariable,
            DSharpBytecodeOperation.ModFields,
            DSharpBytecodeOperation.XorVariables,
            DSharpBytecodeOperation.XorVariableField,
            DSharpBytecodeOperation.XorFieldVariable,
            DSharpBytecodeOperation.XorFields,
            DSharpBytecodeOperation.OrVariables,
            DSharpBytecodeOperation.OrVariableField,
            DSharpBytecodeOperation.OrFields,
            DSharpBytecodeOperation.AndVariables,
            DSharpBytecodeOperation.AndVariableField,
            DSharpBytecodeOperation.AndFields,
            DSharpBytecodeOperation.ShiftLeftVariables,
            DSharpBytecodeOperation.ShiftLeftVariableField,
            DSharpBytecodeOperation.ShiftLeftFieldVariable,
            DSharpBytecodeOperation.ShiftLeftFields,
            DSharpBytecodeOperation.ShiftRightVariables,
            DSharpBytecodeOperation.ShiftRightVariableField,
            DSharpBytecodeOperation.ShiftRightFieldVariable,
            DSharpBytecodeOperation.ShiftRightFields,
        ]);
        private static readonly ReadOnlyCollection<DSharpBytecodeOperation> _conditionalBinaryOperations = new([
            DSharpBytecodeOperation.OrVariables,
            DSharpBytecodeOperation.OrVariableField,
            DSharpBytecodeOperation.OrFields,
            DSharpBytecodeOperation.AndVariables,
            DSharpBytecodeOperation.AndVariableField,
            DSharpBytecodeOperation.AndFields,
            DSharpBytecodeOperation.LessVariables,
            DSharpBytecodeOperation.LessVariableField,
            DSharpBytecodeOperation.LessFields,
            DSharpBytecodeOperation.LessOrEqualVariables,
            DSharpBytecodeOperation.LessOrEqualVariableField,
            DSharpBytecodeOperation.LessOrEqualFields,
            DSharpBytecodeOperation.GreaterVariables,
            DSharpBytecodeOperation.GreaterVariableField,
            DSharpBytecodeOperation.GreaterFields,
            DSharpBytecodeOperation.GreaterOrEqualVariables,
            DSharpBytecodeOperation.GreaterOrEqualVariableField,
            DSharpBytecodeOperation.GreaterOrEqualFields,
        ]);
        private static readonly ReadOnlyDictionary<DSharpBytecodeOperation, DSharpBinaryOperator> _conditionalBinaryOperators = new(new Dictionary<DSharpBytecodeOperation, DSharpBinaryOperator>
        {
            [DSharpBytecodeOperation.OrVariables] = DSharpBinaryOperator.LogicalOr,
            [DSharpBytecodeOperation.OrVariableField] = DSharpBinaryOperator.LogicalOr,
            [DSharpBytecodeOperation.OrFields] = DSharpBinaryOperator.LogicalOr,
            [DSharpBytecodeOperation.AndVariables] = DSharpBinaryOperator.LogicalAnd,
            [DSharpBytecodeOperation.AndVariableField] = DSharpBinaryOperator.LogicalAnd,
            [DSharpBytecodeOperation.AndFields] = DSharpBinaryOperator.LogicalAnd,
            [DSharpBytecodeOperation.LessVariables] = DSharpBinaryOperator.LogicalLess,
            [DSharpBytecodeOperation.LessVariableField] = DSharpBinaryOperator.LogicalLess,
            [DSharpBytecodeOperation.LessFields] = DSharpBinaryOperator.LogicalLess,
            [DSharpBytecodeOperation.LessOrEqualVariables] = DSharpBinaryOperator.LogicalLessOrEquals,
            [DSharpBytecodeOperation.LessOrEqualVariableField] = DSharpBinaryOperator.LogicalLessOrEquals,
            [DSharpBytecodeOperation.LessOrEqualFields] = DSharpBinaryOperator.LogicalLessOrEquals,
            [DSharpBytecodeOperation.GreaterVariables] = DSharpBinaryOperator.LogicalGreater,
            [DSharpBytecodeOperation.GreaterVariableField] = DSharpBinaryOperator.LogicalGreater,
            [DSharpBytecodeOperation.GreaterFields] = DSharpBinaryOperator.LogicalGreater,
            [DSharpBytecodeOperation.GreaterOrEqualVariables] = DSharpBinaryOperator.LogicalGreaterOrEquals,
            [DSharpBytecodeOperation.GreaterOrEqualVariableField] = DSharpBinaryOperator.LogicalGreaterOrEquals,
            [DSharpBytecodeOperation.GreaterOrEqualFields] = DSharpBinaryOperator.LogicalGreaterOrEquals,
        });
        private static readonly ReadOnlyDictionary<DSharpBytecodeOperation, DSharpBinaryOperator> _conditionalInvertedBinaryOperators = new(new Dictionary<DSharpBytecodeOperation, DSharpBinaryOperator>
        {
            [DSharpBytecodeOperation.LessVariables] = DSharpBinaryOperator.LogicalGreaterOrEquals,
            [DSharpBytecodeOperation.LessVariableField] = DSharpBinaryOperator.LogicalGreaterOrEquals,
            [DSharpBytecodeOperation.LessFields] = DSharpBinaryOperator.LogicalGreaterOrEquals,
            [DSharpBytecodeOperation.LessOrEqualVariables] = DSharpBinaryOperator.LogicalGreater,
            [DSharpBytecodeOperation.LessOrEqualVariableField] = DSharpBinaryOperator.LogicalGreater,
            [DSharpBytecodeOperation.LessOrEqualFields] = DSharpBinaryOperator.LogicalGreater,
            [DSharpBytecodeOperation.GreaterVariables] = DSharpBinaryOperator.LogicalLessOrEquals,
            [DSharpBytecodeOperation.GreaterVariableField] = DSharpBinaryOperator.LogicalLessOrEquals,
            [DSharpBytecodeOperation.GreaterFields] = DSharpBinaryOperator.LogicalLessOrEquals,
            [DSharpBytecodeOperation.GreaterOrEqualVariables] = DSharpBinaryOperator.LogicalLess,
            [DSharpBytecodeOperation.GreaterOrEqualVariableField] = DSharpBinaryOperator.LogicalLess,
            [DSharpBytecodeOperation.GreaterOrEqualFields] = DSharpBinaryOperator.LogicalLess,
        });
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
                    new(DSharpBytecodeOperation.Push, typeof(LiteralInstruction), new DSharpLiteralValue(0)),
                    new(DSharpBytecodeOperation.Multiply),
                    new(DSharpBytecodeOperation.PopPreviousTwo)
                ],
                UselessCombinationRemoveMultiplyByOne
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
            ),
            new(
                [
                    new(_loadOperations, true),
                    new(_loadOperations, true),
                    InstructionDefinition.Any,
                    new(DSharpBytecodeOperation.PopPreviousTwo),
                    new(_storeOperations, true),
                    new(DSharpBytecodeOperation.Pop),
                ],
                UselessCombinationStoreValuePop
            ),
            new(
                [
                    new(DSharpBytecodeOperation.PopPreviousTwo),
                    new(_conditionalJumpOperations, true),
                    new(DSharpBytecodeOperation.Pop),
                ],
                UselessCombinationJumpConditionPop
            ),
            new(
                [
                    new(_assignableBinaryOperations, true),
                    new(_nativeStoreOperations, true),
                    new([DSharpBytecodeOperation.Pop, DSharpBytecodeOperation.PopRepeat], true),
                ],
                SimplifyMathOperation
            ),
            new(
                [
                    new(_conditionalBinaryOperations, true),
                    new([DSharpBytecodeOperation.JumpIfTrue, DSharpBytecodeOperation.JumpIfFalse], true),
                    new([DSharpBytecodeOperation.Pop, DSharpBytecodeOperation.PopRepeat], true),
                ],
                SimplifyConditionalJump
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
            Dictionary<Instruction, UselessCombination> startIndexOfUselessCombination = [];
            int offset = 0;
            int maxLength = Math.Min(_uselessCombinations.Count, uselessCombinationsRange.Start.Value + uselessCombinationsRange.End.Value + 1);

            for (int i = startInstruction; i < builder.Instructions.Count; i++)
            {
                for (int c = uselessCombinationsRange.Start.Value; c < maxLength; c++)
                {
                    var uselessCombination = _uselessCombinations[c];

                    if (uselessCombination.SequenceEquals(instructions, i))
                    {
                        startIndexOfUselessCombination.Add(builder.Instructions[i], uselessCombination);
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
                var startIndex = builder.Instructions.IndexOf(info.Key);

                if (startIndex == -1)
                {
                    continue;
                }

                offset += info.Value.Remove(builder, startIndex);
            }

            if (maxCombinationsCount == int.MaxValue && instructions.Count > 0)
            {
                Dictionary<Instruction, List<IReferenceInstruction>> referencesToUselessOperations = [];
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

        private static int SimplifyConditionalJump(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            if (builder.Instructions[startIndex + 1] is not ReferenceInstruction jump ||
                jump.ReferencedInstruction == null ||
                (jump.ReferencedInstruction.Operation != DSharpBytecodeOperation.Pop &&
                 jump.ReferencedInstruction.Operation != DSharpBytecodeOperation.PopRepeat))
            {
                return 0;
            }

            var math = builder.Instructions[startIndex];
            Instruction? newJump = null;

            Instruction? CreateInstruction(DSharpBinaryOperator @operator)
            {
                Instruction? result = null;

                if (math is TwoParametersInstruction twoParameters)
                {
                    result = builder.JumpIfBinaryOperation(@operator, twoParameters.FirstParameter, twoParameters.SecondParameter, jump.ReferencedInstruction);
                }
                else if (math is ParameterMemberInstruction parameterMember &&
                         parameterMember is IDSharpFieldInfo field)
                {
                    result = builder.JumpIfBinaryOperation(@operator, parameterMember.Parameter, field, jump.ReferencedInstruction);
                }
                else if (math is TwoMembersInstruction twoMembers &&
                         twoMembers.FirstMember is IDSharpFieldInfo leftField &&
                         twoMembers.SecondMember is IDSharpFieldInfo rightField)
                {
                    result = builder.JumpIfBinaryOperation(@operator, leftField, rightField, jump.ReferencedInstruction);
                }

                if (result != null)
                {
                    builder.Instructions.Remove(result);
                }

                return result;
            }

            if (jump.Operation == DSharpBytecodeOperation.JumpIfFalse)
            {
                if (!_conditionalInvertedBinaryOperators.TryGetValue(math.Operation, out var invertedBinaryOperator))
                {
                    return 0;
                }

                newJump = CreateInstruction(invertedBinaryOperator);
            }
            else if (jump.Operation == DSharpBytecodeOperation.JumpIfTrue)
            {
                if (!_conditionalBinaryOperators.TryGetValue(math.Operation, out var binaryOperator))
                {
                    return 0;
                }

                newJump = CreateInstruction(binaryOperator);
            }

            if (newJump == null)
            {
                return 0;
            }

            int removeCount = 0;

            var removed = ReplaceInstructionReferences(builder, startIndex); // math
            removeCount += removed;
            startIndex -= removed - 1;

            builder.Instructions[startIndex] = newJump;
            ReplaceInstructionReferences(builder, jump, newJump);

            var popIfNotJumped = builder.Instructions[startIndex + 1];

            int RemovePop(Instruction instruction, int index)
            {
                if (instruction.Operation == DSharpBytecodeOperation.Pop)
                {
                    return ReplaceInstructionReferences(builder, index);
                }
                else if (instruction.Operation == DSharpBytecodeOperation.PopRepeat &&
                         instruction is IndexInstruction indexInstruction)
                {
                    indexInstruction.Index--;

                    if (indexInstruction.Index == 0)
                    {
                        return ReplaceInstructionReferences(builder, index);
                    }
                }

                return 0;
            }

            removeCount += RemovePop(popIfNotJumped, startIndex + 1);

            var referencePopIndex = builder.Instructions.IndexOf(jump.ReferencedInstruction);
            bool isLast = builder.Instructions.Count - 1 == referencePopIndex;
            var removeReferencePopCount = RemovePop(jump.ReferencedInstruction, referencePopIndex);
            removeCount += removeReferencePopCount;

            var referencePopIndexAfterRemove = builder.Instructions.IndexOf(jump.ReferencedInstruction);

            if (referencePopIndexAfterRemove == -1 && newJump is IReferenceInstruction referenceJump)
            {
                referenceJump.ReferencedInstruction = builder.Instructions[referencePopIndex - (removeReferencePopCount - (isLast ? 0 : 1))];
            }

            return removeCount;
        }
        private static int SimplifyMathOperation(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            var math = builder.Instructions[startIndex];
            var store = builder.Instructions[startIndex + 1];
            var pop = builder.Instructions[startIndex + 2];

            if (pop.Operation != DSharpBytecodeOperation.Pop &&
                pop.Operation != DSharpBytecodeOperation.PopRepeat)
            {
                return 0;
            }

            Instruction? newInstruction = null;

            bool CreateAnyOrderAssignment(DSharpBytecodeOperation variablesMath, DSharpBytecodeOperation variableFieldMath, DSharpBytecodeOperation fieldsMath,
                                          Func<IDSharpParameterInfo, IDSharpParameterInfo, TwoParametersInstruction> twoParametersAssignment,
                                          Func<IDSharpParameterInfo, IDSharpFieldInfo, ParameterMemberInstruction> parameterMemberAssignment,
                                          Func<IDSharpFieldInfo, IDSharpParameterInfo, MemberParameterInstruction> memberParameterAssignment,
                                          Func<IDSharpFieldInfo, IDSharpFieldInfo, TwoMembersInstruction> twoMembersAssignment,
                                          [NotNullWhen(true)] out Instruction? assignmentInstruction)
            {
                assignmentInstruction = null;

                if (math.Operation == variablesMath)
                {
                    if (math is not TwoParametersInstruction twoParameters ||
                        store is not ParameterInstruction storeParameter)
                    {
                        return false;
                    }
                    if (storeParameter.Parameter == twoParameters.FirstParameter)
                    {
                        assignmentInstruction = twoParametersAssignment(storeParameter.Parameter, twoParameters.SecondParameter);
                    }
                    else if (storeParameter.Parameter == twoParameters.SecondParameter)
                    {
                        assignmentInstruction = twoParametersAssignment(storeParameter.Parameter, twoParameters.FirstParameter);
                    }
                }
                else if (math.Operation == variableFieldMath)
                {
                    if (math is not ParameterMemberInstruction parameterMember ||
                        parameterMember.MemberInfo is not IDSharpFieldInfo operandField)
                    {
                        return false;
                    }
                    if (store is TypeInstruction typeStore &&
                        typeStore.MemberInfo is IDSharpFieldInfo storeField &&
                        storeField == operandField)
                    {
                        assignmentInstruction = memberParameterAssignment(storeField, parameterMember.Parameter);
                    }
                    else if (store is ParameterInstruction parameterStore &&
                             parameterMember.Parameter == parameterStore.Parameter)
                    {
                        assignmentInstruction = parameterMemberAssignment(parameterStore.Parameter, operandField);
                    }
                }
                else if (math.Operation == fieldsMath)
                {
                    if (math is not TwoMembersInstruction twoMembers ||
                        twoMembers.FirstMember is not IDSharpFieldInfo firstField ||
                        twoMembers.SecondMember is not IDSharpFieldInfo secondField ||
                        store is not TypeInstruction storeMember ||
                        storeMember.MemberInfo is not IDSharpFieldInfo storeField)
                    {
                        return false;
                    }
                    if (storeField == firstField)
                    {
                        assignmentInstruction = twoMembersAssignment(storeField, secondField);
                    }
                    else if (storeField == secondField)
                    {
                        assignmentInstruction = twoMembersAssignment(storeField, firstField);
                    }
                }

                return assignmentInstruction != null;
            }
            bool CreateStrictOrderAssignment(DSharpBytecodeOperation variablesMath, DSharpBytecodeOperation variableFieldMath,
                                             DSharpBytecodeOperation fieldVariableMath, DSharpBytecodeOperation fieldsMath,
                                             Func<IDSharpParameterInfo, IDSharpParameterInfo, TwoParametersInstruction> twoParametersAssignment,
                                             Func<IDSharpParameterInfo, IDSharpFieldInfo, ParameterMemberInstruction> parameterMemberAssignment,
                                             Func<IDSharpFieldInfo, IDSharpParameterInfo, MemberParameterInstruction> memberParameterAssignment,
                                             Func<IDSharpFieldInfo, IDSharpFieldInfo, TwoMembersInstruction> twoMembersAssignment,
                                             [NotNullWhen(true)] out Instruction? assignmentInstruction)
            {
                assignmentInstruction = null;

                if (math.Operation == variablesMath)
                {
                    if (math is not TwoParametersInstruction twoParameters ||
                        store is not ParameterInstruction storeParameter ||
                        twoParameters.FirstParameter != storeParameter.Parameter)
                    {
                        return false;
                    }

                    assignmentInstruction = twoParametersAssignment(twoParameters.FirstParameter, twoParameters.SecondParameter);
                }
                else if (math.Operation == variableFieldMath)
                {
                    if (math is not ParameterMemberInstruction parameterMember ||
                        parameterMember.MemberInfo is not IDSharpFieldInfo operandField ||
                        store is not ParameterInstruction storeParameter ||
                        parameterMember.Parameter != storeParameter.Parameter)
                    {
                        return false;
                    }

                    assignmentInstruction = parameterMemberAssignment(parameterMember.Parameter, operandField);
                }
                else if (math.Operation == fieldVariableMath)
                {
                    if (math is not MemberParameterInstruction memberParameter ||
                        memberParameter.MemberInfo is not IDSharpFieldInfo operandField ||
                        store is not TypeInstruction storeMember ||
                        storeMember.MemberInfo != operandField)
                    {
                        return false;
                    }

                    assignmentInstruction = memberParameterAssignment(operandField, memberParameter.Parameter);
                }
                else if (math.Operation == fieldsMath)
                {
                    if (math is not TwoMembersInstruction twoMembers ||
                        twoMembers.FirstMember is not IDSharpFieldInfo firstField ||
                        twoMembers.SecondMember is not IDSharpFieldInfo secondField ||
                        store is not TypeInstruction storeMember ||
                        storeMember.MemberInfo != firstField)
                    {
                        return false;
                    }

                    assignmentInstruction = twoMembersAssignment(firstField, secondField);
                }

                return assignmentInstruction != null;
            }

            if (CreateAnyOrderAssignment(DSharpBytecodeOperation.AddVariables,
                                         DSharpBytecodeOperation.AddVariableField,
                                         DSharpBytecodeOperation.AddFields,
                                         builder.AssignAdd,
                                         builder.AssignAdd,
                                         builder.AssignAdd,
                                         builder.AssignAdd, out newInstruction) ||
                CreateAnyOrderAssignment(DSharpBytecodeOperation.MultiplyVariables,
                                         DSharpBytecodeOperation.MultiplyVariableField,
                                         DSharpBytecodeOperation.MultiplyFields,
                                         builder.AssignMultiply,
                                         builder.AssignMultiply,
                                         builder.AssignMultiply,
                                         builder.AssignMultiply, out newInstruction) ||
                CreateAnyOrderAssignment(DSharpBytecodeOperation.OrVariables,
                                         DSharpBytecodeOperation.OrVariableField,
                                         DSharpBytecodeOperation.OrFields,
                                         builder.AssignOr,
                                         builder.AssignOr,
                                         builder.AssignOr,
                                         builder.AssignOr, out newInstruction) ||
                CreateAnyOrderAssignment(DSharpBytecodeOperation.AndVariables,
                                         DSharpBytecodeOperation.AndVariableField,
                                         DSharpBytecodeOperation.AndFields,
                                         builder.AssignAnd,
                                         builder.AssignAnd,
                                         builder.AssignAnd,
                                         builder.AssignAnd, out newInstruction) ||
                CreateStrictOrderAssignment(DSharpBytecodeOperation.SubtractVariables,
                                            DSharpBytecodeOperation.SubtractVariableField,
                                            DSharpBytecodeOperation.SubtractFieldVariable,
                                            DSharpBytecodeOperation.SubtractFields,
                                            builder.AssignSubtract,
                                            builder.AssignSubtract,
                                            builder.AssignSubtract,
                                            builder.AssignSubtract, out newInstruction) ||
                CreateStrictOrderAssignment(DSharpBytecodeOperation.DivideVariables,
                                            DSharpBytecodeOperation.DivideVariableField,
                                            DSharpBytecodeOperation.DivideFieldVariable,
                                            DSharpBytecodeOperation.DivideFields,
                                            builder.AssignDivide,
                                            builder.AssignDivide,
                                            builder.AssignDivide,
                                            builder.AssignDivide, out newInstruction) ||
                CreateStrictOrderAssignment(DSharpBytecodeOperation.XorVariables,
                                            DSharpBytecodeOperation.XorVariableField,
                                            DSharpBytecodeOperation.XorFieldVariable,
                                            DSharpBytecodeOperation.XorFields,
                                            builder.AssignXor,
                                            builder.AssignXor,
                                            builder.AssignXor,
                                            builder.AssignXor, out newInstruction) ||
                CreateStrictOrderAssignment(DSharpBytecodeOperation.ShiftLeftVariables,
                                            DSharpBytecodeOperation.ShiftLeftVariableField,
                                            DSharpBytecodeOperation.ShiftLeftFieldVariable,
                                            DSharpBytecodeOperation.ShiftLeftFields,
                                            builder.AssignShiftLeft,
                                            builder.AssignShiftLeft,
                                            builder.AssignShiftLeft,
                                            builder.AssignShiftLeft, out newInstruction) ||
                CreateStrictOrderAssignment(DSharpBytecodeOperation.ShiftRightVariables,
                                            DSharpBytecodeOperation.ShiftRightVariableField,
                                            DSharpBytecodeOperation.ShiftRightFieldVariable,
                                            DSharpBytecodeOperation.ShiftRightFields,
                                            builder.AssignShiftRight,
                                            builder.AssignShiftRight,
                                            builder.AssignShiftRight,
                                            builder.AssignShiftRight, out newInstruction))
            {
            }

            if (newInstruction == null)
            {
                return 0;
            }

            int offset = 1;

            builder.Instructions.Remove(newInstruction);
            ReplaceInstructionReferences(builder, math, newInstruction);
            ReplaceInstructionReferences(builder, store, newInstruction);

            builder.Instructions[startIndex] = newInstruction;
            builder.Instructions.RemoveAt(startIndex + 1);

            if (pop.Operation == DSharpBytecodeOperation.Pop)
            {
                offset += ReplaceInstructionReferences(builder, startIndex + 1);
            }
            else if (pop is IndexInstruction popRepeatInstruction)
            {
                popRepeatInstruction.Index--;

                if (popRepeatInstruction.Index == 0)
                {
                    offset += ReplaceInstructionReferences(builder, startIndex + 1);
                }
            }

            return offset;
        }

        private static int UselessCombinationJumpConditionPop(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            if (builder.Instructions[startIndex + 1] is not ReferenceInstruction referenceInstruction ||
                referenceInstruction.ReferencedInstruction?.Operation != DSharpBytecodeOperation.Pop)
            {
                return 0;
            }

            var removeCount = ReplaceInstructionReferences(builder, startIndex);
            startIndex -= removeCount - 1;

            var popAfterJump = builder.Instructions[startIndex + 1];
            var popAllAfterJump = builder.PopRepeat(3);
            builder.Instructions.Remove(popAllAfterJump);
            builder.Instructions[startIndex + 1] = popAllAfterJump;

            ReplaceInstructionReferences(builder, popAfterJump, popAllAfterJump);

            int referencedInstructionIndex = builder.Instructions.IndexOf(referenceInstruction.ReferencedInstruction);
            var popAllJump = builder.PopRepeat(3);
            builder.Instructions.Remove(popAllJump);
            builder.Instructions[referencedInstructionIndex] = popAllJump;

            ReplaceInstructionReferences(builder, referenceInstruction.ReferencedInstruction, popAllJump);

            return removeCount;
        }
        private static int UselessCombinationStoreValuePop(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            var removeCount = ReplaceInstructionReferences(builder, startIndex + 3);
            startIndex -= removeCount - 1;

            int popIndex = startIndex + 4;
            var oldPop = builder.Instructions[popIndex];
            var popRepeat = builder.PopRepeat(3);

            builder.Instructions.Remove(popRepeat);
            builder.Instructions[popIndex] = popRepeat;

            ReplaceInstructionReferences(builder, oldPop, popRepeat);

            return removeCount;
        }
        private static int UselessCombinationRemoveMultiplyByOne(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            if (builder.Instructions[startIndex] is not LiteralInstruction literalInstruction ||
                !literalInstruction.Value.IsNumber)
            {
                return 0;
            }

            int multiplyValue = -1;

            try
            {
                multiplyValue = literalInstruction.Value.AsNumber<int>();
            }
            catch
            {
            }

            var removeCount = 0;

            if (multiplyValue == 0)
            {
                if (0 >= startIndex || builder.Instructions[startIndex - 1] is not TypeInstruction)
                {
                    return 0;
                }

                var removed = ReplaceInstructionReferences(builder, startIndex - 1);
                startIndex -= removed - 1;
                removeCount += removed;

                for (int i = 0; i < 2; i++)
                {
                    removed = ReplaceInstructionReferences(builder, startIndex);
                    startIndex -= removed - 1;
                    removeCount += removeCount;
                }
            }
            else if (multiplyValue == 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    var removed = ReplaceInstructionReferences(builder, startIndex);
                    startIndex -= removed - 1;
                    removeCount += removed;
                }
            }

            return removeCount;
        }
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

            var startAddMode = builder.InstructionsAddMode;
            var startInsertIndex = builder.InstructionsInsertIndex;

            builder.InstructionsAddMode = DSharpBytecodeInstructionAddMode.Index;
            builder.InstructionsInsertIndex = startIndex;

            // For saving amount of values in stack
            builder.Push(null);

            builder.InstructionsAddMode = startAddMode;
            builder.InstructionsInsertIndex = startInsertIndex;

            return 2;
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
                var removed = ReplaceInstructionReferences(builder, startIndex + 2);
                return 6 + removed;
            }

            var instructionToRemove = builder.Instructions[startIndex + 2];
            IndexInstruction popOffset = new(builder, DSharpBytecodeOperation.PopOffset, 1);
            builder.Instructions[startIndex + 2] = popOffset;

            ReplaceInstructionReferences(builder, instructionToRemove, popOffset);

            return 6;
        }
        private static int UselessCombinationRemoveJumpScope(UselessCombination uselessCombination, DSharpBytecodeBuilder builder, int startIndex)
        {
            var removed = ReplaceInstructionReferences(builder, startIndex);
            startIndex -= removed - 1;
            removed += ReplaceInstructionReferences(builder, startIndex + 1);

            return removed;
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
            int removeCount = 0;

            for (int i = 0; i < 2; i++)
            {
                var removed = ReplaceInstructionReferences(builder, startIndex);
                removeCount += removed;
                startIndex -= removed - 1;
            }

            return removeCount;
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

            var removed = ReplaceInstructionReferences(builder, popInstructionIndex);

            return removed + offset;
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
            return ReplaceInstructionReferences(builder, startIndex);
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

            int removeCount = 0;

            for (int i = startIndex; i < startIndex + 2; i++)
            {
                if (checkReferences)
                {
                    instructions.RemoveAt(startIndex);
                    removeCount++;
                }
                else
                {
                    var removed = ReplaceInstructionReferences(builder, startIndex);
                    removeCount += removed;
                    startIndex -= removed - 1;
                }
            }

            return removeCount;
        }

        private static int RemoveLastInstruction(DSharpBytecodeBuilder builder)
        {
            var instructions = builder.Instructions;

            if (instructions.Count == 0)
            {
                return 0;
            }

            var lastInstruction = instructions[^1];
            int removeCount = 0;

            while (instructions.Count > 1 && instructions[^2].Operation == DSharpBytecodeOperation.SkipNext)
            {
                ReplaceInstructionReferences(builder, instructions[^2], lastInstruction);
                instructions.RemoveAt(instructions.Count - 2);
                removeCount++;
            }

            if (lastInstruction.Operation != DSharpBytecodeOperation.Empty)
            {
                instructions.RemoveAt(instructions.Count - 1);
                var newEmpty = builder.Empty();
                ReplaceInstructionReferences(builder, lastInstruction, newEmpty);
            }

            return removeCount;
        }

        private static int ReplaceInstructionReferences(DSharpBytecodeBuilder builder, int instructionIndex)
        {
            if (instructionIndex == builder.Instructions.Count - 1)
            {
                return RemoveLastInstruction(builder);
            }

            var oldInstruction = builder.Instructions[instructionIndex];
            var previousIndex = instructionIndex - 1;
            var removeCount = 1;

            if (previousIndex >= 0 && builder.Instructions[previousIndex].Operation == DSharpBytecodeOperation.SkipNext)
            {
                var removed = ReplaceInstructionReferences(builder, previousIndex);
                instructionIndex -= removed;
                removeCount += removed;
            }

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

            return removeCount;
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
