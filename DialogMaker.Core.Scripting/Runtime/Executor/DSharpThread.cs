using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# thread
    /// </summary>
    /// <param name="executor">D# virtual machine</param>
    /// <param name="stackCapacity">Stack capacity in items</param>
    public unsafe class DSharpThread(DSharpVm executor, DSharpObjectsContainer objectsContainer, int stackCapacity) : Disposable
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

        #region Controls

        /// <summary>
        /// Start method executing
        /// </summary>
        /// <param name="instance">Object instance that contains method for executing</param>
        /// <param name="methodInfo">Method for executing</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start(DSharpObject* instance, DSharpRuntimeMethodInfo* methodInfo)
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
                    throw new NotImplementedException("Calling external methods not implemented yet");
                }
                if (methodInfo->IsStatic && instance == null)
                {
                    throw new InvalidOperationException("Unable to start executing non static method without object instance");
                }
                if (!methodInfo->IsStatic && instance->Type != methodInfo->DeclaringType)
                {
                    throw new InvalidOperationException("Unable to invoke method with object instance that not declares calling method");
                }

                if (!continueExecuting)
                {
                    var newMethodExecutor = stack.PushMethodExecutor(methodInfo);
                    newMethodExecutor->ObjectInstance = instance;
                    newMethodExecutor->Bytecode = typesProvider.GetRuntimeBytecode(methodInfo);
                    newMethodExecutor->PreviousExecutor = methodExecutor;
                    methodExecutor = newMethodExecutor;

                    // create local variables
                }
                else
                {
                    continueExecuting = false;

                    if (methodExecutor->HaveUnhandledException)
                    {
                        HandleException(methodExecutor->UnhandledException);
                    }
                }

                var callback = methodExecutor->Execute(objectContainer, this);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void Unwind(uint offset = 0)
                {
                    var startStackCount = methodExecutor->StartStackCount;
                    var unhandledException = methodExecutor->UnhandledException;
                    var haveUnhandledException = methodExecutor->HaveUnhandledException;

                    methodExecutor = methodExecutor->PreviousExecutor;
                    instance = methodExecutor->ObjectInstance;
                    methodInfo = methodExecutor->MethodInfo;

                    if (methodExecutor != null)
                    {
                        methodExecutor->HaveUnhandledException = haveUnhandledException;
                        methodExecutor->UnhandledException = unhandledException;
                    }

                    continueExecuting = true;

                    while (stack.Count + offset >= startStackCount)
                    {
                        stack.Pop(offset);
                    }
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
                    uint offset = 0;

                    if (methodInfo->ReturnType != null)
                    {
                        offset = 1;
                    }

                    Unwind(offset);
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.RequiredCallingNextMethod)
                {
                    methodInfo = callback.NextMethod;
                    instance = callback.ObjectInstance;
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
