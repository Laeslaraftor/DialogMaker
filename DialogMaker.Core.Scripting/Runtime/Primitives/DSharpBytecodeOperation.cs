namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# bytecode operation code
    /// </summary>
    public enum DSharpBytecodeOperation : short
    {
        /// <summary>
        /// Push value to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        Push,
        /// <summary>
        /// Remove last value from stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        Pop,
        /// <summary>
        /// Remove value from stack with offset
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        PopOffset,
        /// <summary>
        /// Remove value from stack with offset and repeat it several times
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        PopOffsetRepeat,
        /// <summary>
        /// Remove value from stack and repeat it several times
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        PopRepeat,
        /// <summary>
        /// Remove previous 2 values from stack. This is analog of <c>PopOffsetRepeat 1 2</c>
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        PopPreviousTwo,
        /// <summary>
        /// Replace value in stack at specified index
        /// </summary>
        [ArgsCount(2)]
        [RequestsStackValues(0)]
        StackReplace,
        /// <summary>
        /// Load value from argument to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        LoadArgument,
        /// <summary>
        /// Store last value from stack to argument variable.
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        StoreArgument,
        /// <summary>
        /// Load value from local variable to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        LoadLocal,
        /// <summary>
        /// Store last value from stack to local variable.
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        StoreLocal,
        /// <summary>
        /// Load field value to stack
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        LoadField,
        /// <summary>
        /// Load object instance field value to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        LoadInstanceField,
        /// <summary>
        /// Store last value from stack to field.
        /// Stack:
        /// 0: value
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        StoreField,
        /// <summary>
        /// Store last value from stack to object instance field.
        /// Stack:
        /// 0: value,
        /// 1: instance
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(2)]
        StoreInstanceField,
        /// <summary>
        /// Load current object instance to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        LoadInstance,

        /// <summary>
        /// Call function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        Call,
        /// <summary>
        /// Call and await function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        AwaitCall,
        /// <summary>
        /// Call method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        CallInstance,
        /// <summary>
        /// Call method from instance of object that must be placed in bottom of stack without searching overriding member. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        CallBaseInstance,
        /// <summary>
        /// Call and await method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        AwaitCallInstance,
        /// <summary>
        /// Call and await method from instance of object that must be placed in bottom of stack without searching overriding member. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-2)]
        AwaitCallBaseInstance,
        /// <summary>
        /// Jump to instruction
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(0)]
        Jump,
        /// <summary>
        /// Jump to instruction if last result of comparison is true
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        JumpIfTrue,
        /// <summary>
        /// Jump to instruction if last result of comparison is false
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        JumpIfFalse,
        /// <summary>
        /// Return from current method or function
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        Return,
        /// <summary>
        /// Begins scope. 
        /// All values that added after this operation will be removed after <see cref="EndScope"/>
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        StartScope,
        /// <summary>
        /// Close current scope and remove all its values from stack.
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        EndScope,

        /// <summary>
        /// Casts last value in stack to specified type
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(1)]
        Cast,
        /// <summary>
        /// Addition math operation (+).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Add,
        /// <summary>
        /// Subtraction math operation (-).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Subtract,
        /// <summary>
        /// Multiplication math operation (*).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Multiply,
        /// <summary>
        /// Division math operation (/).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Divide,
        /// <summary>
        /// Mod math operation (%).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Mod,
        /// <summary>
        /// Shift left (<<).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        ShiftLeft,
        /// <summary>
        /// Shift right (>>).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        ShiftRight,
        /// <summary>
        /// Xor (^).
        /// Result will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Xor,
        /// <summary>
        /// Increase last stack value by 1.
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Increment,
        /// <summary>
        /// Decrease last stack value by 1
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Decrement,

        /// <summary>
        /// Logical OR (||). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Or,
        /// <summary>
        /// Logical AND (&&). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        And,
        /// <summary>
        /// Logical EQUALS (==). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Equals,
        /// <summary>
        /// Logical NOT EQUALS (!=). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        NotEquals,
        /// <summary>
        /// Logical LESS (<). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Less,
        /// <summary>
        /// Logical LESS OR EQUAL (<=). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        LessOrEqual,
        /// <summary>
        /// Logical GREATER (>). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        Greater,
        /// <summary>
        /// Logical GREATER OR EQUAL (>=). 
        /// Result of comparison will be added to stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(2)]
        GreaterOrEqual,
        /// <summary>
        /// Logical NOT (!). 
        /// This operation invert last boolean value in stack
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        Not,

        /// <summary>
        /// Create new instance of object, call it's constructor and add it to stack.
        /// Args: constructor or type token
        /// Stack:
        /// 0: args...
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        New,
        /// <summary>
        /// Create new instance of array and add it to stack.
        /// Stack:
        /// 0: size
        /// </summary>
        [ArgsCount(1)]
        [RequestsStackValues(-1)]
        NewArray,
        /// <summary>
        /// Throw exception which must be in last stack value
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(1)]
        Throw,
        /// <summary>
        /// No operation
        /// </summary>
        [ArgsCount(0)]
        [RequestsStackValues(0)]
        Empty,
    }
}
