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

        private readonly DSharpObjectsContainer _objectsContainer = objectsContainer;

        #region Controls

        public void Start(DSharpObject* instance, DSharpRuntimeMethodInfo* methodInfo)
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
                    var newMethodExecutor = stack.PushMethodCalling();
                    newMethodExecutor->ObjectInstance = instance;
                    newMethodExecutor->MethodInfo = methodInfo;
                    newMethodExecutor->Bytecode = typesProvider.GetRuntimeBytecode(methodInfo);
                    newMethodExecutor->PreviousExecutor = methodExecutor;
                    methodExecutor = newMethodExecutor;
                }
                else
                {
                    continueExecuting = false;
                }

                var callback = methodExecutor->Execute(objectContainer, this);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void Unwind()
                {
                    methodExecutor = methodExecutor->PreviousExecutor;
                    instance = methodExecutor->ObjectInstance;
                    methodInfo = methodExecutor->MethodInfo;
                    continueExecuting = true;
                }

                if (callback.Type == DSharpMethodExecutionCallbackType.ExecutionComplete)
                {
                    Unwind();
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.RequiredCallingNextMethod)
                {
                    methodInfo = callback.NextMethod;
                    instance = callback.ObjectInstance;
                }
                else if (callback.Type == DSharpMethodExecutionCallbackType.UnhandledException)
                {
                    Unwind();
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
