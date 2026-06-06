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
        /// Load value from local variable to stack
        /// </summary>
        LoadLocal,
        /// <summary>
        /// Store last value from stack to local variable
        /// </summary>
        StoreLocal,
        /// <summary>
        /// Load field value to stack
        /// </summary>
        LoadField,
        /// <summary>
        /// Store last value from stack to field
        /// </summary>
        StoreField,
        /// <summary>
        /// Load property value to stack
        /// </summary>
        LoadProperty,
        /// <summary>
        /// Store last value from stack to property
        /// </summary>
        StoreProperty,

        /// <summary>
        /// Call function or method
        /// </summary>
        Call,
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
        /// Throw exception which must be in last stack value
        /// </summary>
        Throw,
    }
}
