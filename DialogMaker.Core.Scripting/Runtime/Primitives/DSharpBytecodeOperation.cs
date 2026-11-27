using DialogMaker.Core.Scripting.CodeAnalyzer;
using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# bytecode operation code
    /// </summary>
    [GenerateInformation(EnumValuesInformation.Full)]
    public enum DSharpBytecodeOperation : byte
    {
        #region Default operations

        /// <summary>
        /// Push value to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPushInstructionExecutor))]
        Push,
        /// <summary>
        /// Push null value (all zero bytes) with buffer that have equals size to specified type.
        /// For reference types this buffer have size as pointer
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpPushSizedNullInstructionExecutor))]
        PushSizedNull,
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
        /// Move value on specified amount of indexes that placed on specified offset.
        /// StackMove(int offset, int moveOffset)
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpStackMoveInstructionExecutor))]
        StackMove,
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
        /// Load value from static property to stack.
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadPropertyInstructionExecutor))]
        LoadProperty,

        /// <summary>
        /// Store last value from stack to object instance indexer.
        /// Stack:
        /// 0: instance
        /// 1: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-3)]
        [Executor(typeof(DSharpStorePropertyInstructionExecutor))]
        StoreIndexer,
        /// <summary>
        /// Load value from object instance indexer to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpLoadPropertyInstructionExecutor))]
        LoadIndexer,

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
        /// Load specified method information pointer to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLoadMethodInformationInstructionExecutor))]
        LoadMethodInformation,
        /// <summary>
        /// Load specified type size to stack
        /// </summary>
        [ArgsCount(2)]
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
        /// Get address of variable or field. It works identically to <![CDATA[&someVariable]]>.
        /// First argument is <see cref="DSharpGetAddressMember"/>, second is variable index or field metadata token
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpGetAddressInstructionExecutor))]
        GetAddress,
        /// <summary>
        /// Read value on address that placed on last stack value.
        /// First argument is type metadata token. It works like <c>*pointer</c>
        /// Stack:
        /// 0: address
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpReadOnAddressInstructionExecutor))]
        ReadOnAddress,
        /// <summary>
        /// Write value that placed on last stack value on address before value.
        /// First argument is type metadata token. It works like <c>*pointer = value</c>
        /// Stack:
        /// 0: address
        /// 1: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(2)]
        [Executor(typeof(DSharpStoreOnAddressInstructionExecutor))]
        StoreOnAddress,

        /// <summary>
        /// Call method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpCallInstructionExecutor))]
        Call,
        /// <summary>
        /// Dynamically call method. Generic methods not supported.
        /// This is virtual calling.
        /// Stack:
        /// 0: runtime method info address (<see cref="DSharpRuntimeMethodInfo"/>),
        /// 1: instance or null
        /// 2: args...
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpDynamicCallInstructionExecutor))]
        DynamicCall,
        /// <summary>
        /// Lock last value in stack.
        /// It throws exception if stack value is null or it was not reference type.
        /// If this value already locked then it stops current thread or just wait until it not unlocked
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpLockInstructionExecutor))]
        Lock,
        /// <summary>
        /// Lock last value in stack.
        /// It throws exception if stack value is null or it was not reference type.
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpUnlockInstructionExecutor))]
        Unlock,

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
        /// Boxes last value is stack. 
        /// If value is reference type nothing will happen.
        /// It replaces last value with reference to object
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpBoxInstructionExecutor))]
        Box,
        /// <summary>
        /// Try to cast last value to stack to specified type.
        /// If cast is unavailable it return null, otherwise casted value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpAsInstructionExecutor))]
        As,
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
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpIncrementInstructionExecutor))]
        Increment,
        /// <summary>
        /// Decrease last stack value by 1
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpDecrementInstructionExecutor))]
        Decrement,
        /// <summary>
        /// Invert all bits in number that on last stack value
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpBitInvertInstructionExecutor))]
        BitInvert,

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
        /// Create new array at stack (stackalloc). It allocates memory at stack it push pointer to it
        /// Stack:
        /// 0: size
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpNewStackArrayInstructionExecutor))]
        NewStackArray,
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
        /// <summary>
        /// Create object that stores it ows data by pointer that placed in last value in stack.
        /// First argument is type of creating object. 
        /// Provided pointer should points to buffer with size equals or higher then specified object type.
        /// It creates object at stack without calling any constructors
        /// Stack:
        /// 0: pointer to data
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        [Executor(typeof(DSharpMakeObjectInstructionExecutor))]
        MakeObject,

        #endregion

        #region Extra

        /// <summary>
        /// Sums two variables and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 + value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAddVariablesInstructionExecutor))]
        AddVariables,
        /// <summary>
        /// Subtract second variable from first and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 - value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [Executor(typeof(DSharpSubtractVariablesInstructionExecutor))]
        [RequestsStackValues(0)]
        SubtractVariables,
        /// <summary>
        /// Multiply variables value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 * value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpMultiplyVariablesInstructionExecutor))]
        MultiplyVariables,
        /// <summary>
        /// Subtract first variable value by second variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 / value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpDivideVariablesInstructionExecutor))]
        DivideVariables,
        /// <summary>
        /// Divide first variable value by variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 % value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpModVariablesInstructionExecutor))]
        ModVariables,
        /// <summary>
        /// Apply inversion mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 ^ value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpXorVariablesInstructionExecutor))]
        XorVariables,
        /// <summary>
        /// Apply additive mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 | value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpOrVariablesInstructionExecutor))]
        OrVariables,
        /// <summary>
        /// Apply multiplicative mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 & value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAddVariablesInstructionExecutor))]
        AndVariables,
        /// <summary>
        /// Shift to left first variable value bits by second variable value as offset and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 << value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpShiftLeftVariablesInstructionExecutor))]
        ShiftLeftVariables,
        /// <summary>
        /// Shift to right first variable value bits by second variable value as offset and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 >> value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpShiftRightVariablesInstructionExecutor))]
        ShiftRightVariables,
        /// <summary>
        /// Compares values that stored in variables and push result into stack (boolean)
        /// </summary>
        /// <remarks>
        /// <c>value1 == value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpEqualsVariablesInstructionExecutor))]
        EqualsVariables,
        /// <summary>
        /// Compares values that stored in variables and push opposite result into stack (boolean).
        /// </summary>
        /// <remarks>
        /// <c>value1 != value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpNotEqualsVariablesInstructionExecutor))]
        NotEqualsVariables,
        /// <summary>
        /// Compares first variable less to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 < value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLessVariablesInstructionExecutor))]
        LessVariables,
        /// <summary>
        /// Compares first variable less or equals to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 <= value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpLessOrEqualVariablesInstructionExecutor))]
        LessOrEqualVariables,
        /// <summary>
        /// Compares first variable greater to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 > value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpGreaterVariablesInstructionExecutor))]
        GreaterVariables,
        /// <summary>
        /// Compares first variable greater or equals to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 >= value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpGreaterOrEqualVariablesInstructionExecutor))]
        GreaterOrEqualVariables,

        /// <summary>
        /// Sums two variables and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 + value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAddVariableFieldInstructionExecutor))]
        AddVariableField,
        /// <summary>
        /// Subtract second variable from first and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 - value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpSubtractVariableFieldInstructionExecutor))]
        SubtractVariableField,
        /// <summary>
        /// Multiply variables value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 * value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpMultiplyVariableFieldInstructionExecutor))]
        MultiplyVariableField,
        /// <summary>
        /// Subtract first variable value by second variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 / value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpDivideVariableFieldInstructionExecutor))]
        DivideVariableField,
        /// <summary>
        /// Divide first variable value by variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 % value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpModVariableFieldInstructionExecutor))]
        ModVariableField,
        /// <summary>
        /// Apply inversion mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 ^ value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpXorVariableFieldInstructionExecutor))]
        XorVariableField,
        /// <summary>
        /// Apply additive mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 | value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpOrVariableFieldInstructionExecutor))]
        OrVariableField,
        /// <summary>
        /// Apply multiplicative mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 & value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAndVariableFieldInstructionExecutor))]
        AndVariableField,
        /// <summary>
        /// Shift to left first variable value bits by second variable value as offset and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 << value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpShiftLeftVariableFieldInstructionExecutor))]
        ShiftLeftVariableField,
        /// <summary>
        /// Shift to right first variable value bits by second variable value as offset and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 >> value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpShiftRightVariableFieldInstructionExecutor))]
        ShiftRightVariableField,
        /// <summary>
        /// Compares values that stored in variables and push result into stack (boolean)
        /// </summary>
        /// <remarks>
        /// <c>value1 == value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpEqualsVariableFieldInstructionExecutor))]
        EqualsVariableField,
        /// <summary>
        /// Compares values that stored in variables and push opposite result into stack (boolean).
        /// </summary>
        /// <remarks>
        /// <c>value1 != value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpNotEqualsVariableFieldInstructionExecutor))]
        NotEqualsVariableField,
        /// <summary>
        /// Compares first variable less to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 < value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpLessVariableFieldInstructionExecutor))]
        LessVariableField,
        /// <summary>
        /// Compares first variable less or equals to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 <= value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpLessOrEqualVariableFieldInstructionExecutor))]
        LessOrEqualVariableField,
        /// <summary>
        /// Compares first variable greater to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 > value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpGreaterVariableFieldInstructionExecutor))]
        GreaterVariableField,
        /// <summary>
        /// Compares first variable greater or equals to second and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 >= value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpGreaterOrEqualVariableFieldInstructionExecutor))]
        GreaterOrEqualVariableField,

        /// <summary>
        /// Subtract second variable from first and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 - value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpSubtractFieldVariableInstructionExecutor))]
        SubtractFieldVariable,
        /// <summary>
        /// Subtract first variable value by second variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 / value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpDivideFieldVariableInstructionExecutor))]
        DivideFieldVariable,
        /// <summary>
        /// Divide first variable value by variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 % value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpModFieldVariableInstructionExecutor))]
        ModFieldVariable,
        /// <summary>
        /// Apply inversion mask on first variable value bits by second variable value bits and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 ^ value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpXorFieldVariableInstructionExecutor))]
        XorFieldVariable,
        /// <summary>
        /// Shift to left first variable value bits by second variable value as offset and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 << value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpShiftLeftFieldVariableInstructionExecutor))]
        ShiftLeftFieldVariable,
        /// <summary>
        /// Shift to right first variable value bits by second variable value as offset and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>value1 >> value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpShiftRightFieldVariableInstructionExecutor))]
        ShiftRightFieldVariable,

        /// <summary>
        /// Sums two field and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 + value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAddFieldsInstructionExecutor))]
        AddFields,
        /// <summary>
        /// Subtract second field from first and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 - value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [Executor(typeof(DSharpSubtractFieldsInstructionExecutor))]
        [RequestsStackValues(-2)]
        SubtractFields,
        /// <summary>
        /// Multiply field value and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 * value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpMultiplyFieldsInstructionExecutor))]
        MultiplyFields,
        /// <summary>
        /// Subtract first field value by second field value and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 / value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpDivideFieldsInstructionExecutor))]
        DivideFields,
        /// <summary>
        /// Divide first field value by field value and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 % value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpModFieldsInstructionExecutor))]
        ModFields,
        /// <summary>
        /// Apply inversion mask on first field value bits by second field value bits and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 ^ value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpXorFieldsInstructionExecutor))]
        XorFields,
        /// <summary>
        /// Apply additive mask on first field value bits by second field value bits and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 | value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpOrFieldsInstructionExecutor))]
        OrFields,
        /// <summary>
        /// Apply multiplicative mask on first field value bits by second field value bits and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 & value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAddFieldsInstructionExecutor))]
        AndFields,
        /// <summary>
        /// Shift to left first field value bits by second field value as offset and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 << value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpShiftLeftFieldsInstructionExecutor))]
        ShiftLeftFields,
        /// <summary>
        /// Shift to right first field value bits by second field value as offset and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 >> value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpShiftRightFieldsInstructionExecutor))]
        ShiftRightFields,
        /// <summary>
        /// Compares values that stored in fields and push result into stack (boolean).
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 == value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpEqualsFieldsInstructionExecutor))]
        EqualsFields,
        /// <summary>
        /// Compares values that stored in fields and push opposite result into stack (boolean).
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 != value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpNotEqualsFieldsInstructionExecutor))]
        NotEqualsFields,
        /// <summary>
        /// Compares first field less to second and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 < value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpLessFieldsInstructionExecutor))]
        LessFields,
        /// <summary>
        /// Compares first field less or equals to second and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 <= value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpLessOrEqualFieldsInstructionExecutor))]
        LessOrEqualFields,
        /// <summary>
        /// Compares first field greater to second and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 > value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpGreaterFieldsInstructionExecutor))]
        GreaterFields,
        /// <summary>
        /// Compares first field greater or equals to second and push result into stack.
        /// Stack:
        /// 0: right instance (if non-static field)
        /// 0-1: left instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>value1 >= value2</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpGreaterOrEqualFieldsInstructionExecutor))]
        GreaterOrEqualFields,

        /// <summary>
        /// Sums local variable values and assign value to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignAddLocalLocalInstructionExecutor))]
        AssignAddLocalLocal,
        /// <summary>
        /// Sums field and variable values and assign value to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignAddLocalFieldInstructionExecutor))]
        AssignAddLocalField,
        /// <summary>
        /// Sums field and variable values and assign value to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignAddFieldLocalInstructionExecutor))]
        AssignAddFieldLocal,
        /// <summary>
        /// Sums field values and assign value to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignAddFieldFieldInstructionExecutor))]
        AssignAddFieldField,
        /// <summary>
        /// Subtract second variable value from first and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignSubtractLocalLocalInstructionExecutor))]
        AssignSubtractLocalLocal,
        /// <summary>
        /// Subtract field value from variable and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignSubtractLocalFieldInstructionExecutor))]
        AssignSubtractLocalField,
        /// <summary>
        /// Subtract variable value from field and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignSubtractFieldLocalInstructionExecutor))]
        AssignSubtractFieldLocal,
        /// <summary>
        /// Subtract second field value from first and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignSubtractFieldFieldInstructionExecutor))]
        AssignSubtractFieldField,
        /// <summary>
        /// Multiply variable values and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignMultiplyLocalLocalInstructionExecutor))]
        AssignMultiplyLocalLocal,
        /// <summary>
        /// Multiply variable and field values and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignMultiplyLocalFieldInstructionExecutor))]
        AssignMultiplyLocalField,
        /// <summary>
        /// Multiply variable and field values and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignMultiplyFieldLocalInstructionExecutor))]
        AssignMultiplyFieldLocal,
        /// <summary>
        /// Multiply field values and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignMultiplyFieldFieldInstructionExecutor))]
        AssignMultiplyFieldField,
        /// <summary>
        /// Divide first variable value by second and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignDivideLocalLocalInstructionExecutor))]
        AssignDivideLocalLocal,
        /// <summary>
        /// Divide variable value by field and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignDivideLocalFieldInstructionExecutor))]
        AssignDivideLocalField,
        /// <summary>
        /// Divide field value by variable and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignDivideFieldLocalInstructionExecutor))]
        AssignDivideFieldLocal,
        /// <summary>
        /// Divide first field value by second and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignDivideFieldFieldInstructionExecutor))]
        AssignDivideFieldField,
        /// <summary>
        /// Divide first variable by second and assign remainder to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignModLocalLocalInstructionExecutor))]
        AssignModLocalLocal,
        /// <summary>
        /// Divide variable by field and assign remainder to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignModLocalFieldInstructionExecutor))]
        AssignModLocalField,
        /// <summary>
        /// Divide field by variable and assign remainder to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignModFieldLocalInstructionExecutor))]
        AssignModFieldLocal,
        /// <summary>
        /// Divide first field by second and assign remainder to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignModFieldFieldInstructionExecutor))]
        AssignModFieldField,
        /// <summary>
        /// Apply second variable value inversion bit mask to first and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignXorLocalLocalInstructionExecutor))]
        AssignXorLocalLocal,
        /// <summary>
        /// Apply field value inversion bit mask to variable and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignXorLocalFieldInstructionExecutor))]
        AssignXorLocalField,
        /// <summary>
        /// Apply variable value inversion bit mask to field and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignXorFieldLocalInstructionExecutor))]
        AssignXorFieldLocal,
        /// <summary>
        /// Apply second field value inversion bit mask to first and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignXorFieldFieldInstructionExecutor))]
        AssignXorFieldField,
        /// <summary>
        /// Apply second variable value additive bit mask to first and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignOrLocalLocalInstructionExecutor))]
        AssignOrLocalLocal,
        /// <summary>
        /// Apply field value additive bit mask to variable and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignOrLocalFieldInstructionExecutor))]
        AssignOrLocalField,
        /// <summary>
        /// Apply variable value additive bit mask to field and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignOrFieldLocalInstructionExecutor))]
        AssignOrFieldLocal,
        /// <summary>
        /// Apply second field value additive bit mask to first and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignOrFieldFieldInstructionExecutor))]
        AssignOrFieldField,
        /// <summary>
        /// Apply second variable value multiplicative bit mask to first and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignAndLocalLocalInstructionExecutor))]
        AssignAndLocalLocal,
        /// <summary>
        /// Apply field value multiplicative bit mask to variable and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignAndLocalFieldInstructionExecutor))]
        AssignAndLocalField,
        /// <summary>
        /// Apply variable value multiplicative bit mask to field and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignAndFieldLocalInstructionExecutor))]
        AssignAndFieldLocal,
        /// <summary>
        /// Apply second field value multiplicative bit mask to first and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignAndFieldFieldInstructionExecutor))]
        AssignAndFieldField,
        /// <summary>
        /// Shift left first variable value by second offset and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignShiftLeftLocalLocalInstructionExecutor))]
        AssignShiftLeftLocalLocal,
        /// <summary>
        /// Shift left variable value by field offset and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignShiftLeftLocalFieldInstructionExecutor))]
        AssignShiftLeftLocalField,
        /// <summary>
        /// Shift left field value by variable offset and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignShiftLeftFieldLocalInstructionExecutor))]
        AssignShiftLeftFieldLocal,
        /// <summary>
        /// Shift left first field value by second offset and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignShiftLeftFieldFieldInstructionExecutor))]
        AssignShiftLeftFieldField,
        /// <summary>
        /// Shift right first variable value by second offset and assign to first variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpAssignShiftRightLocalLocalInstructionExecutor))]
        AssignShiftRightLocalLocal,
        /// <summary>
        /// Shift right variable value by field offset and assign to variable
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignShiftRightLocalFieldInstructionExecutor))]
        AssignShiftRightLocalField,
        /// <summary>
        /// Shift right field value by variable offset and assign to field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpAssignShiftRightFieldLocalInstructionExecutor))]
        AssignShiftRightFieldLocal,
        /// <summary>
        /// Shift right first field value by second offset and assign to first field
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpAssignShiftRightFieldFieldInstructionExecutor))]
        AssignShiftRightFieldField,

        /// <summary>
        /// Invert variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>-value or !value</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpInvertVariableInstructionExecutor))]
        InvertVariable,
        /// <summary>
        /// Increment variable value by 1.
        /// First argument is variable index,
        /// second argument is flag (bool) that indicates when result is need to be pushed to stack
        /// </summary>
        /// <remarks>
        /// <c>value++</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpIncrementVariableInstructionExecutor))]
        IncrementVariable,
        /// <summary>
        /// Decrement variable value by 1.
        /// First argument is variable index,
        /// second argument is flag (bool) that indicates when result is need to be pushed to stack
        /// </summary>
        /// <remarks>
        /// <c>value--</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpDecrementVariableInstructionExecutor))]
        DecrementVariable,
        /// <summary>
        /// Inverts bits in variable value and push result into stack
        /// </summary>
        /// <remarks>
        /// <c>~value</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpBitInvertVariableInstructionExecutor))]
        BitInvertVariable,

        /// <summary>
        /// Invert field value and push result into stack.
        /// Stack:
        /// 0: instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>-field or !field</c>
        /// </remarks>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpInvertFieldInstructionExecutor))]
        InvertField,
        /// <summary>
        /// Increment field value by 1.
        /// First argument is field metadata token,
        /// second argument is flag (bool) that indicates when result is need to be pushed to stack.
        /// Stack:
        /// 0: instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>field++</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpIncrementFieldInstructionExecutor))]
        IncrementField,
        /// <summary>
        /// Decrement variable value by 1.
        /// First argument is field metadata token,
        /// second argument is flag (bool) that indicates when result is need to be pushed to stack.
        /// Stack:
        /// 0: instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>field--</c>
        /// </remarks>
        [ArgsCount(2)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpDecrementFieldInstructionExecutor))]
        DecrementField,
        /// <summary>
        /// Inverts bits in field value and push result into stack.
        /// Stack:
        /// 0: instance (if non-static field)
        /// </summary>
        /// <remarks>
        /// <c>~field</c>
        /// </remarks>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpBitInvertFieldInstructionExecutor))]
        BitInvertField,

        /// <summary>
        /// Jump to instruction at specified index when first variable value equals to second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalEqualsLocalInstructionExecutor))]
        JumpIfLocalEqualsLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value not equals to second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalNotEqualsLocalInstructionExecutor))]
        JumpIfLocalNotEqualsLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value or second is true
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalOrLocalInstructionExecutor))]
        JumpIfLocalOrLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value and second is true
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalAndLocalInstructionExecutor))]
        JumpIfLocalAndLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value equals less then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalLessLocalInstructionExecutor))]
        JumpIfLocalLessLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value less or equal then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalLessOrEqualLocalInstructionExecutor))]
        JumpIfLocalLessOrEqualLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value greater then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalGreaterLocalInstructionExecutor))]
        JumpIfLocalGreaterLocal,
        /// <summary>
        /// Jump to instruction at specified index when first variable value greater or equal then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalGreaterOrEqualLocalInstructionExecutor))]
        JumpIfLocalGreaterOrEqualLocal,

        /// <summary>
        /// Jump to instruction at specified index when first variable value equals to field value
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalEqualsFieldInstructionExecutor))]
        JumpIfLocalEqualsField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value not equals to field value
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalNotEqualsFieldInstructionExecutor))]
        JumpIfLocalNotEqualsField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value or field value is true
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalOrFieldInstructionExecutor))]
        JumpIfLocalOrField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value and field value is true
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(0)]
        [Executor(typeof(DSharpJumpIfLocalAndFieldInstructionExecutor))]
        JumpIfLocalAndField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value equals less then field value
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalLessFieldInstructionExecutor))]
        JumpIfLocalLessField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value less or equal then field value
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalLessOrEqualFieldInstructionExecutor))]
        JumpIfLocalLessOrEqualField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value greater then field value
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalGreaterFieldInstructionExecutor))]
        JumpIfLocalGreaterField,
        /// <summary>
        /// Jump to instruction at specified index when first variable value greater or equal then field value
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-1)]
        [Executor(typeof(DSharpJumpIfLocalGreaterOrEqualFieldInstructionExecutor))]
        JumpIfLocalGreaterOrEqualField,

        /// <summary>
        /// Jump to instruction at specified index when first field value equals to second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldEqualsFieldInstructionExecutor))]
        JumpIfFieldEqualsField,
        /// <summary>
        /// Jump to instruction at specified index when first field value not equals to second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldNotEqualsFieldInstructionExecutor))]
        JumpIfFieldNotEqualsField,
        /// <summary>
        /// Jump to instruction at specified index when first field value or second is true
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldOrFieldInstructionExecutor))]
        JumpIfFieldOrField,
        /// <summary>
        /// Jump to instruction at specified index when first field value and second is true
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldAndFieldInstructionExecutor))]
        JumpIfFieldAndField,
        /// <summary>
        /// Jump to instruction at specified index when first field value equals less then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldLessFieldInstructionExecutor))]
        JumpIfFieldLessField,
        /// <summary>
        /// Jump to instruction at specified index when first field value less or equal then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldLessOrEqualFieldInstructionExecutor))]
        JumpIfFieldLessOrEqualField,
        /// <summary>
        /// Jump to instruction at specified index when first field value greater then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldGreaterFieldInstructionExecutor))]
        JumpIfFieldGreaterField,
        /// <summary>
        /// Jump to instruction at specified index when first field value greater or equal then second
        /// </summary>
        [ArgsCount(3)]
        [RequestsStackValues(-2)]
        [Executor(typeof(DSharpJumpIfFieldGreaterOrEqualFieldInstructionExecutor))]
        JumpIfFieldGreaterOrEqualField,

        #endregion
    }
}
