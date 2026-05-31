using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    /// <summary>
    /// Script token type
    /// </summary>
    public enum DSharpTokenType
    {
        /// <summary>
        /// Function or method
        /// </summary>
        [Keyword("func")]
        Func,
        /// <summary>
        /// Variable with unknown type (object)
        /// </summary>
        [Keyword("var")]
        Var,
        /// <summary>
        /// Return keyword.
        /// </summary>
        [Keyword("return")]
        Return,
        /// <summary>
        /// If keyword
        /// </summary>
        [Keyword("if")]
        If,
        /// <summary>
        /// Else keyword
        /// </summary>
        [Keyword("else")]
        Else,
        /// <summary>
        /// While keyword
        /// </summary>
        [Keyword("while")]
        While,
        /// <summary>
        /// For keyword
        /// </summary>
        [Keyword("for")]
        For,
        /// <summary>
        /// Break keyword for stopping loop
        /// </summary>
        [Keyword("break")]
        Break,
        /// <summary>
        /// Continue keyword for skipping iteration
        /// </summary>
        [Keyword("continue")]
        Continue,
        /// <summary>
        /// Await keyword for waiting function or method. 
        /// This works only if function or method have some async operation (wait(number) or other await calling).
        /// Calling async method without this keyword starts new thread which execute called function or method,
        /// execution continues without waiting completion method
        /// </summary>
        [Keyword("await")]
        Await,
        /// <summary>
        /// Extern keyword for function or methods which must me added by compiler or execution handler/
        /// </summary>
        [Keyword(ObjectDeclarationNode.ExternModifier)]
        Extern,
        /// <summary>
        /// Static keyword for function or methods which allows to access without object instance
        /// </summary>
        [Keyword(ObjectDeclarationNode.StaticModifier)]
        Static,
        /// <summary>
        /// Struct keyword
        /// </summary>
        [Keyword("struct")]
        Struct,
        /// <summary>
        /// Struct keyword
        /// </summary>
        [Keyword("class")]
        Class,
        /// <summary>
        /// Enum keyword
        /// </summary>
        [Keyword("enum")]
        Enum,
        /// <summary>
        /// Attribute keyword. Works like struct keyword, but it purpose for defining attributes
        /// </summary>
        [Keyword("attribute")]
        Attribute,
        /// <summary>
        /// True keyword (true)
        /// </summary>
        [Keyword("true")]
        True,
        /// <summary>
        /// False keyword (false)
        /// </summary>
        [Keyword("false")]
        False,
        /// <summary>
        /// Null keyword (null)
        /// </summary>
        [Keyword("null")]
        Null,
        /// <summary>
        /// As keyword. Works identical to C#
        /// </summary>
        [Keyword("as")]
        As,
        /// <summary>
        /// As keyword. Works identical to C#
        /// </summary>
        [Keyword("is")]
        Is,
        /// <summary>
        /// Object keyword for variable or property with unknown type. This is root of all type system
        /// </summary>
        [Keyword("object")]
        Object,
        /// <summary>
        /// String keyword (string)
        /// </summary>
        [Keyword("string")]
        String,
        /// <summary>
        /// Number keyword (int, float, double)
        /// </summary>
        [Keyword("number")]
        Number,
        /// <summary>
        /// Bool keyword (bool)
        /// </summary>
        [Keyword("bool")]
        Bool,
        /// <summary>
        /// Getter keyword
        /// </summary>
        [Keyword("get")]
        Get,
        /// <summary>
        /// Setter keyword
        /// </summary>
        [Keyword("set")]
        Set,
        /// <summary>
        /// Lambda keyword (=>)
        /// </summary>
        [Keyword("=>")]
        Lambda,
        /// <summary>
        /// New keyword
        /// </summary>
        [Keyword("new")]
        New,
        /// <summary>
        /// Namespace keyword
        /// </summary>
        [Keyword("namespace")]
        Namespace,

        /// <summary>
        /// Public access modifier
        /// </summary>
        [Keyword("public")]
        Public,
        /// <summary>
        /// Private access modifier
        /// </summary>
        [Keyword("private")]
        Private,
        /// <summary>
        /// Protected access modifier
        /// </summary>
        [Keyword("protected")]
        Protected,

        /// <summary>
        /// Assign operator (=)
        /// </summary>
        Assign,            // =
        /// <summary>
        /// Plus operator (+)
        /// </summary>
        Plus, 
        /// <summary>
        /// Minus operator (-)
        /// </summary>
        Minus, 
        /// <summary>
        /// Multiply operator (*)
        /// </summary>
        Multiply,
        /// <summary>
        /// Divide operator (/)
        /// </summary>
        Divide, 
        /// <summary>
        /// Mod operator (%)
        /// </summary>
        Mod,
        /// <summary>
        /// Equal operator (==)
        /// </summary>
        Equal, 
        /// <summary>
        /// Not equal operator (!=)
        /// </summary>
        NotEqual,
        /// <summary>
        /// Less operator (<)
        /// </summary>
        Less, 
        /// <summary>
        /// Greater operator (>)
        /// </summary>
        Greater, 
        /// <summary>
        /// Less or equal operator (<=)
        /// </summary>
        LessEqual, 
        /// <summary>
        /// Greater or equal operator (>=)
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// Logical multiply operator (&&)
        /// </summary>
        And, 
        /// <summary>
        /// Logical addition operator (||)
        /// </summary>
        Or, 
        /// <summary>
        /// Logical NOT operator (!)
        /// </summary>
        Not,
        /// <summary>
        /// Increment by 1 operator (++)
        /// </summary>
        Increment, 
        /// <summary>
        /// Decrement by 1 operator (--)
        /// </summary>
        Decrement,
        /// <summary>
        /// Assign with addition operator (+=)
        /// </summary>
        PlusAssign, 
        /// <summary>
        /// Assign with subtract operator (-=)
        /// </summary>
        MinusAssign,
        /// <summary>
        /// Assign with divide operator (/=)
        /// </summary>
        DivideAssign, 
        /// <summary>
        /// Assign with multiply operator (*=)
        /// </summary>
        MultiplyAssign,

        /// <summary>
        /// (
        /// </summary>
        LeftParen, 
        /// <summary>
        /// )
        /// </summary>
        RightParen,
        /// <summary>
        /// {
        /// </summary>
        LeftBrace, 
        /// <summary>
        /// }
        /// </summary>
        RightBrace,
        /// <summary>
        /// [
        /// </summary>
        LeftBracket, 
        /// <summary>
        /// ]
        /// </summary>
        RightBracket,
        /// <summary>
        /// ,
        /// </summary>
        Comma, 
        /// <summary>
        /// .
        /// </summary>
        Dot, 
        /// <summary>
        /// ;
        /// </summary>
        Semicolon, 
        /// <summary>
        /// :
        /// </summary>
        Colon,
        /// <summary>
        /// Special symbol that means string with ignoring \
        /// </summary>
        At,
        /// <summary>
        /// Question mark means type which can be null
        /// </summary>
        Question,

        // Значения
        /// <summary>
        /// Name of variable, function, method, etc.
        /// </summary>
        Identifier,
        /// <summary>
        /// String value
        /// </summary>
        StringLiteral,
        /// <summary>
        /// Number literal it's just a number
        /// </summary>
        NumberLiteral,
        /// <summary>
        /// Single line comment (//)
        /// </summary>
        Comment,
        /// <summary>
        /// Multiline comment which placed between /* and */
        /// </summary>
        MultilineComment,

        /// <summary>
        /// End of file
        /// </summary>
        EndOfFile
    }
}
