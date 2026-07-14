using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# bytecode operation code
    /// </summary>
    public enum DSharpBytecodeOperation : byte
    {
        /// <summary>
        /// Push value to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPushInstructionExecutor))]
        Push,
        /// <summary>
        /// Remove last value from stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPopInstructionExecutor))]
        Pop,
        /// <summary>
        /// Remove value from stack with offset
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPopOffsetInstructionExecutor))]
        PopOffset,
        /// <summary>
        /// Remove value from stack with offset and repeat it several times
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPopOffsetRepeatInstructionExecutor))]
        PopOffsetRepeat,
        /// <summary>
        /// Remove value from stack and repeat it several times
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPopRepeatInstructionExecutor))]
        PopRepeat,
        /// <summary>
        /// Remove previous 2 values from stack. This is analog of <c>PopOffsetRepeat 1 2</c>
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPopPreviousTwoInstructionExecutor))]
        PopPreviousTwo,
        /// <summary>
        /// Load value from local variable to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadLocalInstructionExecutor))]
        LoadLocal,
        /// <summary>
        /// Store last value from stack to local variable
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpStoreLocalInstructionExecutor))]
        StoreLocal,
        /// <summary>
        /// Load field value to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadFieldInstructionExecutor))]
        LoadField,
        /// <summary>
        /// Load object instance field value to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpLoadInstanceFieldInstructionExecutor))]
        LoadInstanceField,
        /// <summary>
        /// Store last value from stack to field.
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpStoreFieldInstructionExecutor))]
        StoreField,
        /// <summary>
        /// Store last value from stack to object instance field.
        /// Stack:
        /// 0: value,
        /// 1: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpStoreInstanceFieldInstructionExecutor))]
        StoreInstanceField,

        /// <summary>
        /// Store last value from stack to static property.
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpStorePropertyInstructionExecutor))]
        StoreProperty,
        /// <summary>
        /// Store last value from stack to object instance property.
        /// Stack:
        /// 0: instance
        /// 1: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpStoreInstancePropertyInstructionExecutor))]
        StoreInstanceProperty,
        /// <summary>
        /// Store last value from stack to object instance property without searching overriding member. 
        /// Stack:
        /// 0: instance
        /// 1: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpStoreBaseInstancePropertyInstructionExecutor))]
        StoreBaseInstanceProperty,
        /// <summary>
        /// Load value from static property to stack.
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadPropertyInstructionExecutor))]
        LoadProperty,
        /// <summary>
        /// Load value from object instance property to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpLoadInstancePropertyInstructionExecutor))]
        LoadInstanceProperty,
        /// <summary>
        /// Load value from object instance property to stack without searching overriding member. 
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpLoadBaseInstancePropertyInstructionExecutor))]
        LoadBaseInstanceProperty,

        /// <summary>
        /// Store last value from stack to object instance indexer.
        /// Stack:
        /// 0: instance
        /// 1: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-3)]
        [Executor(typeof(DSharpStoreIndexerInstructionExecutor))]
        StoreIndexer,
        /// <summary>
        /// Store last value from stack to object instance indexer without searching overriding member. 
        /// Stack:
        /// 0: instance
        /// 1: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-3)]
        [Executor(typeof(DSharpStoreBaseIndexerInstructionExecutor))]
        StoreBaseIndexer,
        /// <summary>
        /// Load value from object instance indexer to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpLoadIndexerInstructionExecutor))]
        LoadIndexer,
        /// <summary>
        /// Load value from object instance indexer to stack without searching overriding member. 
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpLoadBaseIndexerInstructionExecutor))]
        LoadBaseIndexer,

        /// <summary>
        /// Load current object instance to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadInstanceInstructionExecutor))]
        LoadInstance,
        /// <summary>
        /// Load specified type information pointer to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadTypeInformationInstructionExecutor))]
        LoadTypeInformation,
        /// <summary>
        /// Load specified type size to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadTypeSizeInstructionExecutor))]
        LoadTypeSize,
        /// <summary>
        /// Load index of current instruction
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadCurrentInstructionIndexInstructionExecutor))]
        LoadCurrentInstructionIndex,

        /// <summary>
        /// Call function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpCallInstructionExecutor))]
        Call,
        /// <summary>
        /// Call and await function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAwaitCallInstructionExecutor))]
        AwaitCall,
        /// <summary>
        /// Call method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpCallInstanceInstructionExecutor))]
        CallInstance,
        /// <summary>
        /// Call method from instance of object that must be placed in bottom of stack without searching overriding member. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpCallBaseInstanceInstructionExecutor))]
        CallBaseInstance,
        /// <summary>
        /// Call and await method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAwaitCallInstanceInstructionExecutor))]
        AwaitCallInstance,
        /// <summary>
        /// Call and await method from instance of object that must be placed in bottom of stack without searching overriding member. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAwaitCallBaseInstanceInstructionExecutor))]
        AwaitCallBaseInstance,
        /// <summary>
        /// Call generic function or static generic method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(-2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpGenericCallInstructionExecutor))]
        GenericCall,
        /// <summary>
        /// Call and await generic function or static generic method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(-2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAwaitGenericCallInstructionExecutor))]
        AwaitGenericCall,
        /// <summary>
        /// Call generic method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(-2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpGenericCallInstanceInstructionExecutor))]
        GenericCallInstance,
        /// <summary>
        /// Call generic method from instance of object that must be placed in bottom of stack without searching overriding member. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(-2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpGenericCallBaseInstanceInstructionExecutor))]
        GenericCallBaseInstance,
        /// <summary>
        /// Call and await generic method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(-2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAwaitGenericCallInstanceInstructionExecutor))]
        AwaitGenericCallInstance,
        /// <summary>
        /// Call and await generic method from instance of object that must be placed in bottom of stack without searching overriding member. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(-2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAwaitGenericCallBaseInstanceInstructionExecutor))]
        AwaitGenericCallBaseInstance,

        /// <summary>
        /// Jump to instruction on index that stores in stack last value
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpJumpIndexedInstructionExecutor))]
        JumpIndexed,
        /// <summary>
        /// Jump to instruction
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpInstructionExecutor))]
        Jump,
        /// <summary>
        /// Jump to instruction if last result of comparison is true
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpJumpIfTrueInstructionExecutor))]
        JumpIfTrue,
        /// <summary>
        /// Jump to instruction if last result of comparison is false
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpJumpIfFalseInstructionExecutor))]
        JumpIfFalse,
        /// <summary>
        /// Skips next instruction
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpSkipNextInstructionExecutor))]
        SkipNext,
        /// <summary>
        /// Return from current method or function
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpReturnInstructionExecutor))]
        Return,
        /// <summary>
        /// Begins scope. 
        /// All values that added after this operation will be removed after <see cref="EndScope"/>
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpStartScopeInstructionExecutor))]
        StartScope,
        /// <summary>
        /// Close current scope and remove all its values from stack.
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpEndScopeInstructionExecutor))]
        EndScope,

        /// <summary>
        /// Start try operation scope
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpStartTryingInstructionExecutor))]
        StartTrying,
        /// <summary>
        /// End try operation scope and remove all registered to it catch and finally operations
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpStopTryingInstructionExecutor))]
        StopTrying,
        /// <summary>
        /// Register catch block that handles all exceptions.
        /// This requires index of instruction that begin catch block
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpRegisterCatchInstructionExecutor))]
        RegisterCatch,
        /// <summary>
        /// Register catch block that handles all exceptions that inherited or equals to specified type.
        /// This requires index of instruction that begin catch block and type of exception
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpRegisterTypedCatchInstructionExecutor))]
        RegisterTypedCatch,
        /// <summary>
        /// Register finally block that executes after try and catch block.
        /// This requires index of instruction that begin catch block.
        /// End of finally block should contains return - any finally block can contains only 1 return statement at end
        /// for returning from finally block back
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpRegisterFinallyInstructionExecutor))]
        RegisterFinally,
        /// <summary>
        /// Call current finally block
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpFinallyInstructionExecutor))]
        Finally,

        /// <summary>
        /// Casts last value in stack to specified type
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpCastInstructionExecutor))]
        Cast,
        /// <summary>
        /// Addition math operation (+).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpAddInstructionExecutor))]
        Add,
        /// <summary>
        /// Subtraction math operation (-).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpSubtractInstructionExecutor))]
        Subtract,
        /// <summary>
        /// Multiplication math operation (*).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpMultiplyInstructionExecutor))]
        Multiply,
        /// <summary>
        /// Division math operation (/).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpDivideInstructionExecutor))]
        Divide,
        /// <summary>
        /// Mod math operation (%).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpModInstructionExecutor))]
        Mod,
        /// <summary>
        /// Shift left (<<).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpShiftLeftInstructionExecutor))]
        ShiftLeft,
        /// <summary>
        /// Shift right (>>).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpShiftRightInstructionExecutor))]
        ShiftRight,
        /// <summary>
        /// Xor (^).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpXorInstructionExecutor))]
        Xor,
        /// <summary>
        /// Increase last stack value by 1.
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpIncrementInstructionExecutor))]
        Increment,
        /// <summary>
        /// Decrease last stack value by 1
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpDecrementInstructionExecutor))]
        Decrement,

        /// <summary>
        /// OR (|). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpOrInstructionExecutor))]
        Or,
        /// <summary>
        /// AND (&). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpAndInstructionExecutor))]
        And,
        /// <summary>
        /// Logical EQUALS (==). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpEqualsInstructionExecutor))]
        Equals,
        /// <summary>
        /// Logical NOT EQUALS (!=). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpNotEqualsInstructionExecutor))]
        NotEquals,
        /// <summary>
        /// Logical LESS (<). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpLessInstructionExecutor))]
        Less,
        /// <summary>
        /// Logical LESS OR EQUAL (<=). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpLessOrEqualInstructionExecutor))]
        LessOrEqual,
        /// <summary>
        /// Logical GREATER (>). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpGreaterInstructionExecutor))]
        Greater,
        /// <summary>
        /// Logical GREATER OR EQUAL (>=). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpGreaterOrEqualInstructionExecutor))]
        GreaterOrEqual,
        /// <summary>
        /// Logical NOT (!). 
        /// This operation invert last boolean value in stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpNotInstructionExecutor))]
        Not,

        /// <summary>
        /// Create new instance of object, call it's constructor and add it to stack.
        /// Args: constructor or type token
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpNewInstructionExecutor))]
        New,
        /// <summary>
        /// Create new instance of array and add it to stack.
        /// Stack:
        /// 0: size
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpNewArrayInstructionExecutor))]
        NewArray,
        /// <summary>
        /// Throw exception which must be in last stack value
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpThrowInstructionExecutor))]
        Throw,
        /// <summary>
        /// No operation
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpEmptyInstructionExecutor))]
        Empty,
    }
}
