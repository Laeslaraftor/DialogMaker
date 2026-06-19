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
        Push,
        /// <summary>
        /// Remove last value from stack
        /// </summary>
        Pop,
        /// <summary>
        /// Remove value from stack with offset
        /// </summary>
        PopOffset,
        /// <summary>
        /// Remove value from stack with offset and repeat it several times
        /// </summary>
        PopOffsetRepeat,
        /// <summary>
        /// Remove value from stack and repeat it several times
        /// </summary>
        PopRepeat,
        /// <summary>
        /// Remove previous 2 values from stack. This is analog of <c>PopOffsetRepeat 1 2</c>
        /// </summary>
        PopPreviousTwo,
        /// <summary>
        /// Load value from argument to stack
        /// </summary>
        LoadArgument,
        /// <summary>
        /// Store last value from stack to argument variable.
        /// Stack:
        /// 0: value
        /// </summary>
        StoreArgument,
        /// <summary>
        /// Load value from local variable to stack
        /// </summary>
        LoadLocal,
        /// <summary>
        /// Store last value from stack to local variable.
        /// Stack:
        /// 0: value
        /// </summary>
        StoreLocal,
        /// <summary>
        /// Load field value to stack
        /// </summary>
        LoadField,
        /// <summary>
        /// Load object instance field value to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        LoadInstanceField,
        /// <summary>
        /// Store last value from stack to field.
        /// Stack:
        /// 0: value
        /// </summary>
        StoreField,
        /// <summary>
        /// Store last value from stack to object instance field.
        /// Stack:
        /// 0: value,
        /// 1: instance
        /// </summary>
        StoreInstanceField,
        /// <summary>
        /// Load property value to stack
        /// </summary>
        LoadProperty,
        /// <summary>
        /// Load object instance property value to stack.
        /// Stack:
        /// 0: instance
        /// </summary>
        LoadInstanceProperty,
        /// <summary>
        /// Store last value from stack to property.
        /// Stack:
        /// 0: value
        /// </summary>
        StoreProperty,
        /// <summary>
        /// Store last value from stack to object instance property.
        /// Stack:
        /// 0: value,
        /// 1: instance
        /// </summary>
        StoreInstanceProperty,
        /// <summary>
        /// Load item value from array to stack.
        /// Stack:
        /// 0: index,
        /// 1: instance
        /// </summary>
        LoadArrayItem,
        /// <summary>
        /// Store last value from stack to array. 
        /// Stack:
        /// 0: value
        /// 1: index,
        /// 2: instance,
        /// </summary>
        StoreArrayItem,
        /// <summary>
        /// Load current object instance to stack
        /// </summary>
        LoadInstance,

        /// <summary>
        /// Call function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        Call,
        /// <summary>
        /// Call and await function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        AwaitCall,
        /// <summary>
        /// Call method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        CallInstance,
        /// <summary>
        /// Call and await method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        AwaitCallInstance,
        /// <summary>
        /// Jump to instruction
        /// </summary>
        Jump,
        /// <summary>
        /// Jump to instruction if last result of comparison is true
        /// </summary>
        JumpIfTrue,
        /// <summary>
        /// Jump to instruction if last result of comparison is false
        /// </summary>
        JumpIfFalse,
        /// <summary>
        /// Return from current method or function
        /// </summary>
        Return,

        /// <summary>
        /// Addition math operation (+).
        /// Result will be added to stack
        /// </summary>
        Add,
        /// <summary>
        /// Subtraction math operation (-).
        /// Result will be added to stack
        /// </summary>
        Subtract,
        /// <summary>
        /// Multiplication math operation (*).
        /// Result will be added to stack
        /// </summary>
        Multiply,
        /// <summary>
        /// Division math operation (/).
        /// Result will be added to stack
        /// </summary>
        Divide,
        /// <summary>
        /// Mod math operation (%).
        /// Result will be added to stack
        /// </summary>
        Mod,
        /// <summary>
        /// Increase last stack value by 1.
        /// </summary>
        Increment,
        /// <summary>
        /// Decrease last stack value by 1
        /// </summary>
        Decrement,

        /// <summary>
        /// Logical OR (||). 
        /// Result of comparison will be added to stack
        /// </summary>
        Or,
        /// <summary>
        /// Logical AND (&&). 
        /// Result of comparison will be added to stack
        /// </summary>
        And,
        /// <summary>
        /// Logical EQUALS (==). 
        /// Result of comparison will be added to stack
        /// </summary>
        Equals,
        /// <summary>
        /// Logical NOT EQUALS (!=). 
        /// Result of comparison will be added to stack
        /// </summary>
        NotEquals,
        /// <summary>
        /// Logical LESS (<). 
        /// Result of comparison will be added to stack
        /// </summary>
        Less,
        /// <summary>
        /// Logical LESS OR EQUAL (<=). 
        /// Result of comparison will be added to stack
        /// </summary>
        LessOrEqual,
        /// <summary>
        /// Logical GREATER (>). 
        /// Result of comparison will be added to stack
        /// </summary>
        Greater,
        /// <summary>
        /// Logical GREATER OR EQUAL (>=). 
        /// Result of comparison will be added to stack
        /// </summary>
        GreaterOrEqual,
        /// <summary>
        /// Logical NOT (!). 
        /// This operation invert last boolean value in stack
        /// </summary>
        Not,

        /// <summary>
        /// Create new instance of object, call it's constructor and add it to stack.
        /// Args: constructor or type token
        /// Stack:
        /// 0: args...
        /// </summary>
        New,
        /// <summary>
        /// Create new instance of array and add it to stack.
        /// Stack:
        /// 0: size
        /// </summary>
        NewArray,
        /// <summary>
        /// Throw exception which must be in last stack value
        /// </summary>
        Throw,
        /// <summary>
        /// No operation
        /// </summary>
        Empty,
    }
}
