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
        /// 0: index,
        /// 1: instance,
        /// 2: value
        /// </summary>
        StoreArrayItem,

        /// <summary>
        /// Call function or static method.
        /// Stack:
        /// 0: args...
        /// </summary>
        Call,
        /// <summary>
        /// Call method from instance of object that must be placed in bottom of stack. 
        /// Stack:
        /// 0: instance,
        /// 1: args...
        /// </summary>
        CallInstance,
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
        /// Await next calling. After this operations needs to call method or function
        /// </summary>
        Await,

        /// <summary>
        /// Addition math operation (+)
        /// </summary>
        Add,
        /// <summary>
        /// Subtraction math operation (-)
        /// </summary>
        Subtract,
        /// <summary>
        /// Multiplication math operation (*)
        /// </summary>
        Multiply,
        /// <summary>
        /// Division math operation (/)
        /// </summary>
        Divide,
        /// <summary>
        /// Mod math operation (%)
        /// </summary>
        Mod,

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
        /// Result of operation will be added to stack
        /// </summary>
        Not,

        /// <summary>
        /// Create new instance of object, call it's constructor and add it to stack
        /// </summary>
        New,
        /// <summary>
        /// Create new instance of array and add it to stack
        /// </summary>
        NewArray,
        /// <summary>
        /// Throw exception which must be in last stack value
        /// </summary>
        Throw,
    }
}
