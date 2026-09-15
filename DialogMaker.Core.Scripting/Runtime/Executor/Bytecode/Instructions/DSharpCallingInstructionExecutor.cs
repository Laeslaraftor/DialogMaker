using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    public unsafe abstract class DSharpCallingInstructionExecutor<T> : DSharpInstructionExecutor
        where T : unmanaged
    {
        #region Controls

        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            var member = (T*)instruction.Arguments[0];
            var callingType = (DSharpMethodCallingType)instruction.Arguments[1];
            UnmanagedArray<Pointer<DSharpMetadataToken>> genericParameters;

            if (instruction.Arguments.Length > 2)
            {
                genericParameters = instruction.Arguments.Slice(2).Cast<Pointer<DSharpMetadataToken>>();
            }
            else
            {
                genericParameters = default;
            }

            member = ReplaceMember(instruction, ref context, member);

            DSharpObject* instance = null;
            var argumentsCount = (uint)GetArgumentsCount(member);

            if (!IsStatic(member))
            {
                if (CheckStackValues(instruction, context, (int)argumentsCount + 1, out var error))
                {
                    return error;
                }

                instance = GetInstance(context, argumentsCount, out error);

                if (instance == null)
                {
                    return error;
                }
            }

            return Execute(instruction, ref context, new()
            {
                Member = member,
                Instance = instance,
                CallingType = callingType,
                GenericParameters = genericParameters,
                ArgumentsCount = argumentsCount
            });
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            stream->Read<DSharpMethodCallingType>();
            bool hasGenerics = stream->Read<bool>();
            int replacesCount = 0;

            if (hasGenerics)
            {
                replacesCount += stream->Read<int>() * 2;

                for (int i = 0; i < replacesCount; i++)
                {
                    stream->Read<DSharpMetadataToken>();
                }
            }

            return replacesCount + 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var methodToken = stream->Read<DSharpMetadataToken>();
            var callingType = stream->Read<DSharpMethodCallingType>();
            bool hasGenerics = stream->Read<bool>();

            arguments[0] = (nint)GetMember(typesProvider, methodToken);
            arguments[1] = (nint)callingType;

            if (hasGenerics)
            {
                var replacesCount = stream->Read<int>() * 2;

                for (int i = 2; i < replacesCount + 2; i++)
                {
                    var typeToken = stream->Read<DSharpMetadataToken>();
                    arguments[i] = (nint)typesProvider.GetMember(typeToken);
                }
            }
        }

        protected abstract DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, CallingExecutionParameters parameters);

        protected abstract T* GetMember(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken);
        protected abstract T* ReplaceMember(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, T* member);
        protected abstract bool IsStatic(T* member);
        protected virtual int GetArgumentsCount(T* member)
        {
            return 0;
        }

        #endregion

        #region Static

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UnmanagedArray<DSharpExecutionLocalVariable> CreateArguments(DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodInfo, uint offset = 0)
        {
            return CreateArguments(context, methodInfo, default, offset).Arguments;
        }
        internal static ArgumentsInfo CreateArguments(DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodInfo, UnmanagedArray<Pointer<DSharpMetadataToken>> genericParameters, uint offset = 0)
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
            UnmanagedDictionary<Pointer<DSharpMetadataToken>, Pointer<DSharpMetadataToken>> generics = new(argsFrame.StackPointer + variablesSize, genericParametersCount);

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
                replaceType = context.ReplaceMember(replaceType);

                generics.Add(genericType, replaceType);
            }

            return new()
            {
                Arguments = arguments,
                GenericParameters = generics
            };
        }


        #endregion

        #region Structs

        public struct CallingExecutionParameters
        {
            public T* Member;
            public DSharpObject* Instance;
            public DSharpMethodCallingType CallingType;
            public UnmanagedArray<Pointer<DSharpMetadataToken>> GenericParameters;
            public uint ArgumentsCount;
        }
        internal struct ArgumentsInfo
        {
            public UnmanagedArray<DSharpExecutionLocalVariable> Arguments;
            public UnmanagedDictionary<Pointer<DSharpMetadataToken>, Pointer<DSharpMetadataToken>> GenericParameters;
        }

        #endregion
    }
}
