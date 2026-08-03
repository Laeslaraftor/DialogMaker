using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Call"/> operation
    /// </summary>
    public class DSharpCallInstructionExecutor : DSharpMethodInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* runtimeInfo)
        {
            return Call(instruction, ref context, runtimeInfo, false, false);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Call"/> operation executor
        /// </summary>
        public static readonly DSharpCallInstructionExecutor Instance = new();

        internal static unsafe DSharpMethodExecutionCallback Call(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* method, bool isInstance, bool isBase, UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>>? genericParameters = null, uint extraScopeOffset = 0)
        {
            var parametersCount = method->ParametersType.Length;

            if (isInstance)
            {
                parametersCount++;
            }
            if (CheckStackValues(instruction, context, parametersCount, out var error))
            {
                return error;
            }

            DSharpObject* instance = null;

            if (isInstance)
            {
                instance = GetInstance(context, (uint)parametersCount - 1, out error);

                if (instance == null)
                {
                    return error;
                }
                if (!isBase && method->CanBeOverriden)
                {
                    if (instance->Type->OverridenMethods.TryGetValue(method, out var endPointMethod))
                    {
                        method = endPointMethod;
                    }
                    else if (method->DeclaringType->ObjectType == DSharpObjectType.Interface ||
                             method->IsAbstract)
                    {
                        return context.ThrowExecutionException($"Unable to find end-point method for \"{method->ToString()}\"");
                    }
                }
            }

            var argumentsInfo = CreateArguments(context, method, genericParameters ?? default, 0);

            return DSharpMethodExecutionCallback.Call(instance, method, argumentsInfo.GenericParameters, argumentsInfo.Arguments, extraScopeOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe UnmanagedArray<DSharpExecutionLocalVariable> CreateArguments(DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodInfo, uint offset = 0)
        {
            return CreateArguments(context, methodInfo, default, offset).Arguments;
        }
        internal static unsafe ArgumentsInfo CreateArguments(DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodInfo, UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> genericParameters, uint offset = 0)
        {
            var parametersCount = methodInfo->ParametersType.Length;
            var genericParametersCount = genericParameters.Length;
            var genericTypes = methodInfo->GenericTypes.Cast<Pointer<DSharpRuntimeTypeInfo>>();

            if (parametersCount == 0 && genericParametersCount == 0)
            {
                return new();
            }
            if (genericParametersCount % 2 != 0)
            {
                throw new ArgumentException($"Generic parameters count should be even, got {genericParametersCount}");
            }

            genericParametersCount /= 2;
            var variablesSize = sizeof(DSharpExecutionLocalVariable) * parametersCount;
            var argsFrame = *context.Stack.Push(DSharpStackValueType.MethodParametersBuffer, variablesSize + 
                                                                                             sizeof(UnmanagedPair<Pointer<DSharpRuntimeTypeInfo>, Pointer<DSharpRuntimeTypeInfo>>) * genericParametersCount);
            UnmanagedArray<DSharpExecutionLocalVariable> arguments = new(argsFrame.StackPointer, parametersCount);
            UnmanagedDictionary<Pointer<DSharpRuntimeTypeInfo>, Pointer<DSharpRuntimeTypeInfo>> generics = new(argsFrame.StackPointer + variablesSize, genericParametersCount);

            for (int i = 0; i < parametersCount; i++)
            {
                var peekOffset = (uint)(parametersCount - 1 - i) + offset;
                var frame = context.Stack.PeekOnlyValues(peekOffset);
                var parameterInfo = methodInfo->ParametersType[i];
                parameterInfo.Type = context.ReplaceType(parameterInfo.Type);

                arguments[i] = new()
                {
                    ParameterInfo = parameterInfo,
                    Buffer = frame
                };
            }

            genericParametersCount *= 2;
            for (int i = 0; i < genericParametersCount; i += 2)
            {
                var genericType = genericParameters[i].AsPointer();
                var replaceType = genericParameters[i + 1].AsPointer();
                replaceType = context.ReplaceType(replaceType);

                generics.Add(genericType, replaceType);
            }

            return new()
            {
                Arguments = arguments,
                GenericParameters = generics
            };
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion

        #region Structs

        internal struct ArgumentsInfo
        {
            public UnmanagedArray<DSharpExecutionLocalVariable> Arguments;
            public UnmanagedDictionary<Pointer<DSharpRuntimeTypeInfo>, Pointer<DSharpRuntimeTypeInfo>> GenericParameters;
        }

        #endregion
    }
}
