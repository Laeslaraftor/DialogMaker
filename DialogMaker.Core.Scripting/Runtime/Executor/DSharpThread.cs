using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# thread
    /// </summary>
    /// <param name="executor">D# virtual machine</param>
    /// <param name="stackCapacity">Stack capacity in items</param>
    public unsafe class DSharpThread(DSharpVm executor, DSharpObjectsContainer objectsContainer, DSharpVmMemoryManager memoryManager, IDSharpExternalMethodsProvider externalMethodsProvider, int stackCapacity) : Disposable
    {
        /// <summary>
        /// D# virtual machine
        /// </summary>
        public DSharpVm Executor { get; } = executor;
        /// <summary>
        /// Current thread stack
        /// </summary>
        public DSharpStack Stack { get; } = new(memoryManager, executor.RuntimeTypesProvider, stackCapacity);
        /// <summary>
        /// Is current thread executing
        /// </summary>
        public bool IsExecuting { get; private set; }
        /// <summary>
        /// Last exception that was thrown
        /// </summary>
        public Exception? LastException { get; private set; }

        private readonly DSharpVmMemoryManager _memoryManager = memoryManager;
        private readonly DSharpObjectsContainer _objectsContainer = objectsContainer;
        private readonly IDSharpExternalMethodsProvider _externalMethodsProvider = externalMethodsProvider;
        private Thread? _thread;

        #region Controls

        public void Start(IDSharpMethodInfo methodInfo) => Start(null, methodInfo);
        public void Start(DSharpObject* instance, IDSharpMethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
            {
                var method = Executor.RuntimeTypesProvider.GetMethod(methodInfo.MetadataToken);
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
        public void Start(DSharpObject* instance, UnmanagedDictionary<Pointer<DSharpRuntimeTypeInfo>, Pointer<DSharpRuntimeTypeInfo>> genericParameters, UnmanagedArray<DSharpExecutionLocalVariable> arguments, DSharpRuntimeMethodInfo* methodInfo)
        {
            if (IsExecuting)
            {
                throw new InvalidOperationException("Thread already started");
            }

            IsExecuting = true;
            Thread thread = new(() =>
            {
                try
                {
                    ThreadLoop(instance, genericParameters, arguments, methodInfo);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                    LastException = error;
                }
                finally
                {
                    IsExecuting = false;
                }
            })
            {
                IsBackground = true,
            };
            _thread = thread;

            thread.Start();
        }

        private void ThreadLoop(DSharpObject* instance, UnmanagedDictionary<Pointer<DSharpRuntimeTypeInfo>, Pointer<DSharpRuntimeTypeInfo>> genericParameters, UnmanagedArray<DSharpExecutionLocalVariable> arguments, DSharpRuntimeMethodInfo* methodInfo)
        {
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
                        DSharpExternalCallingArgs args = new(instance, methodInfo, genericParameters, arguments, stack, Executor.Assembly);
                        var result = externalMethod(args);
                        uint popOffset = 0;

                        if (result != null)
                        {
                            popOffset = 1;

                            if (result != DSharpExternalMethodResult.Stack)
                            {
                                var resultValue = result.Value;

                                if (resultValue.IsObject)
                                {
                                    stack.PushReference(resultValue.AsObject());
                                }
                                else if (resultValue.IsLiteralValue)
                                {
                                    var literalValue = resultValue.AsLiteralValue();

                                    if (literalValue.IsString)
                                    {
                                        var strObject = objectContainer.CreateString(literalValue.AsString());
                                        stack.PushReference(strObject);
                                    }
                                    else
                                    {
                                        stack.Push(literalValue);
                                    }
                                }
                                else
                                {
                                    stack.PushNull();
                                }
                            }
                        }
                        if (stack.Count > popOffset)
                        {
                            var lastValue = stack.Peek(popOffset);

                            if (lastValue.ValueType == DSharpStackValueType.MethodParametersBuffer)
                            {
                                stack.Pop(popOffset);
                            }
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
                if (!methodInfo->IsStatic && !instance->Type->IsInheritFrom(methodInfo->DeclaringType))
                {
                    throw new InvalidOperationException("Unable to invoke method with object instance that not declares calling method");
                }

                if (!continueExecuting)
                {
                    var bytecode = typesProvider.GetRuntimeBytecode(methodInfo);
                    int variablesCount = bytecode->Variables.Length;
                    uint catchFinallyCount = bytecode->CatchBlocksCount + bytecode->FinallyBlocksCount;
                    int extraSize = variablesCount * sizeof(DSharpExecutionLocalVariable) +
                                    (int)bytecode->ScopesCount * sizeof(DSharpStack.Scope) +
                                    (int)catchFinallyCount * sizeof(DSharpTryCatchFinallyDescription) +
                                    (int)bytecode->FinallyBlocksCount * sizeof(uint);
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
                    newMethodExecutor->TryCatchFinallyDescriptions = builder.AllocateArray<DSharpTryCatchFinallyDescription>((int)catchFinallyCount);
                    newMethodExecutor->NextReturnInstructions = builder.AllocateArray<uint>((int)bytecode->FinallyBlocksCount);
                    newMethodExecutor->LocalScopes = builder.AllocateArray<DSharpStack.Scope>((int)bytecode->ScopesCount);
                    newMethodExecutor->InstructionIndex = 0;
                    newMethodExecutor->HaveUnhandledException = false;
                    newMethodExecutor->UnhandledException = null;
                    newMethodExecutor->CurrentTryCatchFinallyId = 0;
                    newMethodExecutor->NowClosingTryCatchFinallyBlock = false;

                    if (methodInfo->MethodType == DSharpMethodType.Initializer &&
                        !methodInfo->IsStatic)
                    {
                        if (instance->IsInitialized)
                        {
                            throw new InvalidOperationException($"Unable to call initialized on initialized object: {instance->ToString()}");
                        }

                        instance->IsInitialized = true;
                    }

                    for (int i = arguments.Length; i < bytecode->Variables.Length; i++)
                    {
                        var variable = bytecode->Variables[i];

                        if (genericParameters.TryGetValue(variable.Type, out var newVariableType))
                        {
                            variable.Type = newVariableType.Value;
                        }

                        newMethodExecutor->LocalVariables[i] = DSharpExecutionLocalVariable.Create(stack, variable);
                    }
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        newMethodExecutor->LocalVariables[i] = arguments[i];
                    }

                    methodExecutor = newMethodExecutor;
                }
                else
                {
                    continueExecuting = false;

                    if (methodExecutor->HaveUnhandledException && !methodExecutor->NowClosingTryCatchFinallyBlock)
                    {
                        if (!HandleException(methodExecutor->UnhandledException))
                        {
                            continue;
                        }
                    }

                    var lastCallback = methodExecutor->LastCallback;

                    if (lastCallback != null &&
                        lastCallback.Value.Type == DSharpMethodExecutionCallbackType.InitializeObject &&
                        lastCallback.Value.ObjectInstance->Type->Initializer != null)
                    {
                        var newCallback = lastCallback.Value;
                        newCallback.Type = DSharpMethodExecutionCallbackType.RequiredCallingNextMethod;
                        methodExecutor->LastCallback = newCallback;
                        SetNextMethod(newCallback);
                        continue;
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
                void SetException(DSharpObject* exception)
                {
                    methodExecutor->HaveUnhandledException = true;
                    methodExecutor->UnhandledException = exception;
                }
                bool HandleException(DSharpObject* exception)
                {
                    if (methodExecutor->NowClosingTryCatchFinallyBlock)
                    {
                        methodExecutor->NowClosingTryCatchFinallyBlock = false;
                        exception = methodExecutor->UnhandledException;
                    }
                    if (DSharpMethodExecutor.TryFindCatchBlockForException(methodExecutor, exception, out var catchBlock))
                    {
                        if (catchBlock.IsFinallyBlock)
                        {
                            SetException(exception);
                            methodExecutor->NowClosingTryCatchFinallyBlock = true;
                            methodExecutor->InstructionIndex = catchBlock.InstructionIndex;
                            continueExecuting = true;
                            return true;
                        }
                        if (exception != null)
                        {
                            var frame = stack.Peek();

                            if (frame.ValueType != DSharpStackValueType.Reference ||
                                frame.ReadReference() != (nint)exception)
                            {
                                stack.PushReference(exception);
                            }
                        }

                        methodExecutor->HaveUnhandledException = false;
                        methodExecutor->InstructionIndex = catchBlock.InstructionIndex;

                        if (!methodExecutor->ContainsFinallyBlock(catchBlock.TryCatchFinallyBlockId))
                        {
                            DSharpExecutionContext.EndTryCatchFinally(methodExecutor);
                        }

                        continueExecuting = true;

                        return true;
                    }
                    else if (methodExecutor->PreviousExecutor != null)
                    {
                        SetException(exception);
                        Unwind();

                        return false;
                    }
                    else
                    {
                        if (exception == null)
                        {
                            throw new DSharpExecutionEngineException($"Unhandled exception at \"{methodInfo->ToString()}\":{methodExecutor->InstructionIndex}");
                        }

                        var message = DSharpObjectConverter.GetMessage(exception);
                        throw new DSharpExecutionEngineException($"Unhandled exception \"{exception->ToString()}\": {message}{Environment.NewLine}   at \"{methodInfo->ToString()}\":{methodExecutor->InstructionIndex}", exception);
                    }
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void SetNextMethod(DSharpMethodExecutionCallback callback)
                {
                    methodInfo = callback.NextMethod;
                    instance = callback.ObjectInstance;
                    arguments = callback.CallingArguments;
                    genericParameters = callback.CallingGenericParameters;
                    continueExecuting = false;
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
                else if (callback.Type == DSharpMethodExecutionCallbackType.InitializeObject)
                {
                    if (callback.ObjectInstance == null)
                    {
                        throw new InvalidOperationException("Unable initialize object instance when instance not provided");
                    }
                    if (callback.ObjectInstance->Type->Initializer != null)
                    {
                        methodInfo = callback.ObjectInstance->Type->Initializer;
                        instance = callback.ObjectInstance;
                        arguments = default;
                        genericParameters = default;

                        continue;
                    }

                    SetNextMethod(callback);
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.RequiredCallingNextMethod)
                {
                    SetNextMethod(callback);
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.UnhandledException)
                {
                    HandleException(callback.UnhandledException);
                }
            }
            while (methodExecutor != null);
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
