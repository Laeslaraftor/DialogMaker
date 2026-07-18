using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# thread
    /// </summary>
    /// <param name="executor">D# virtual machine</param>
    /// <param name="stackCapacity">Stack capacity in items</param>
    public unsafe class DSharpThread(DSharpVm executor, DSharpObjectsContainer objectsContainer, IDSharpExternalMethodsProvider externalMethodsProvider, int stackCapacity) : Disposable
    {
        /// <summary>
        /// D# virtual machine
        /// </summary>
        public DSharpVm Executor { get; } = executor;
        /// <summary>
        /// Current thread stack
        /// </summary>
        public DSharpStack Stack { get; } = new(stackCapacity);
        /// <summary>
        /// Is current thread executing
        /// </summary>
        public bool IsExecuting { get; private set; }

        private readonly DSharpObjectsContainer _objectsContainer = objectsContainer;
        private readonly IDSharpExternalMethodsProvider _externalMethodsProvider = externalMethodsProvider;

        #region Controls

        public void Start(DSharpObject* instance, IDSharpMethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
            {
                var method = Executor.RuntimeTypesProvider.GetStaticMethod(methodInfo.MetadataToken);
                Start(null, default, default, method);
                return;
            }
            if (instance == null)
            {
                throw new ArgumentNullException($"Non static method \"{methodInfo}\" requires object instance", nameof(instance));
            }
            if (!instance->Type->TryGetMethod(methodInfo.MetadataToken, out var runtimeMethod))
            {
                throw new InvalidOperationException($"Unable to find runtime information about method \"{methodInfo}\"");
            }

            Start(instance, default, default, runtimeMethod);
        }

        /// <summary>
        /// Start method executing
        /// </summary>
        /// <param name="instance">Object instance that contains method for executing</param>
        /// <param name="genericParameters">Method generic parameters</param>
        /// <param name="arguments">Method calling arguments</param>
        /// <param name="methodInfo">Method for executing</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start(DSharpObject* instance, UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters, UnmanagedArray<DSharpExecutionLocalVariable> arguments, DSharpRuntimeMethodInfo* methodInfo)
        {
            if (IsExecuting)
            {
                return;
            }

            IsExecuting = true;
            var typesProvider = Executor.RuntimeTypesProvider;
            var objectContainer = _objectsContainer;
            var stack = Stack;
            DSharpMethodExecutor* methodExecutor = null;
            bool continueExecuting = false;

            do
            {
                if (methodInfo->IsExtern)
                {
                    if (typesProvider.Assembly.GetType(methodInfo->MetadataToken) is not IDSharpMethodInfo assemblyMethodInfo)
                    {
                        throw new InvalidOperationException($"Unable to find information about method: {methodInfo->MetadataToken}");
                    }

                    var externalMethod = _externalMethodsProvider.GetMethod(assemblyMethodInfo);

                    if (externalMethod != null)
                    {
                        var result = externalMethod(instance, methodInfo, genericParameters, arguments);

                        if (result != null)
                        {
                            throw new NotImplementedException("Handling value from external method not implemented yet");
                        }

                        instance = methodExecutor->ObjectInstance;
                        methodInfo = methodExecutor->MethodInfo;
                        arguments = methodExecutor->Arguments;
                        genericParameters = methodExecutor->GenericParameters;
                        continueExecuting = true;
                        continue;
                    }

                    throw new NotImplementedException($"External method for \"{assemblyMethodInfo}\" not found");
                }
                if (!methodInfo->IsStatic && instance == null)
                {
                    throw new InvalidOperationException("Unable to start executing non static method without object instance");
                }
                if (!methodInfo->IsStatic && instance->Type != methodInfo->DeclaringType)
                {
                    throw new InvalidOperationException("Unable to invoke method with object instance that not declares calling method");
                }

                if (!continueExecuting)
                {
                    var bytecode = typesProvider.GetRuntimeBytecode(methodInfo);
                    int variablesCount = arguments.Length + bytecode->Variables.Length;
                    int extraSize = variablesCount * sizeof(DSharpExecutionLocalVariable) +
                                    (int)bytecode->ScopesCount * sizeof(DSharpStack.Scope) +
                                    (int)bytecode->TryingBlocksCount * sizeof(DSharpCatchBlock);
                    var newMethodExecutor = stack.PushMethodExecutor(methodInfo, extraSize);
                    MemoryBuilder builder = new((nint)newMethodExecutor + sizeof(DSharpMethodExecutor), extraSize);

                    if (arguments.Length > 0)
                    {
                        newMethodExecutor->Scope.StackCount--;
                    }

                    newMethodExecutor->ObjectInstance = instance;
                    newMethodExecutor->Bytecode = bytecode;
                    newMethodExecutor->GenericParameters = genericParameters;
                    newMethodExecutor->Arguments = arguments;
                    newMethodExecutor->PreviousExecutor = methodExecutor;
                    newMethodExecutor->LocalVariables = builder.AllocateArray<DSharpExecutionLocalVariable>(variablesCount);
                    newMethodExecutor->CatchBlocks = builder.AllocateArray<DSharpCatchBlock>((int)bytecode->TryingBlocksCount);
                    newMethodExecutor->LocalScopes = builder.AllocateArray<DSharpStack.Scope>((int)bytecode->ScopesCount);

                    for (int i = 0; i < arguments.Length; i++)
                    {
                        newMethodExecutor->LocalVariables[i] = arguments[i];
                    }
                    for (int i = 0; i < bytecode->Variables.Length; i++)
                    {
                        newMethodExecutor->LocalVariables[i + arguments.Length] = DSharpExecutionLocalVariable.Create(stack, bytecode->Variables.GetItemReference(i));
                    }

                    methodExecutor = newMethodExecutor;
                }
                else
                {
                    continueExecuting = false;

                    if (methodExecutor->HaveUnhandledException)
                    {
                        HandleException(methodExecutor->UnhandledException);
                    }
                }

                var callback = DSharpMethodExecutor.Execute(methodExecutor, objectContainer, this);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void Unwind(uint offset = 0)
                {
                    var methodScope = methodExecutor->Scope;
                    var unhandledException = methodExecutor->UnhandledException;
                    var haveUnhandledException = methodExecutor->HaveUnhandledException;

                    methodExecutor = methodExecutor->PreviousExecutor;

                    if (methodExecutor != null)
                    {
                        genericParameters = methodExecutor->GenericParameters;
                        instance = methodExecutor->ObjectInstance;
                        methodInfo = methodExecutor->MethodInfo;
                        arguments = methodExecutor->Arguments;
                        methodExecutor->HaveUnhandledException = haveUnhandledException;
                        methodExecutor->UnhandledException = unhandledException;
                    }

                    continueExecuting = true;
                    stack.CloseScope(methodScope, offset);
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void HandleException(DSharpObject* exception)
                {
                    if (methodExecutor->TryFindCatchBlockForException(exception, out var catchBlock))
                    {
                        if (exception != null)
                        {
                            var frame = stack.Peek();

                            if (frame.ValueType != DSharpStackValueType.Reference ||
                                frame.Read<nint>() != (nint)exception)
                            {
                                stack.PushReference(exception);
                            }
                        }

                        methodExecutor->HaveUnhandledException = false;
                        methodExecutor->InstructionIndex = catchBlock.InstructionIndex;
                        continueExecuting = true;
                    }
                    else
                    {
                        Unwind();
                    }
                }

                if (callback.Type == DSharpMethodExecutionCallbackType.ExecutionComplete)
                {
                    Unwind(0);
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.Returned)
                {
                    uint offset = methodInfo->ReturnType != null ? 1u : 0u;
                    Unwind(offset);
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.RequiredCallingNextMethod)
                {
                    methodInfo = callback.NextMethod;
                    instance = callback.ObjectInstance;
                    arguments = callback.CallingArguments;
                    genericParameters = callback.CallingGenericParameters;
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.UnhandledException)
                {
                    HandleException(callback.UnhandledException);
                }
            }
            while (methodExecutor != null);

            IsExecuting = false;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Default size of stack for thread in frames.
        /// 1KB per frame
        /// </summary>
        public const int DefaultStackCapacity = 1024;

        #endregion
    }
}
