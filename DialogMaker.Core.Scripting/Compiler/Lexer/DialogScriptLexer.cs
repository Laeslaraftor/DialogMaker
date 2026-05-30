using System.Collections;
using System.Collections.ObjectModel;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    /// <summary>
    /// Dialog script lexer
    /// </summary>
    /// <param name="source"></param>
    public class DialogScriptLexer(string source) : IEnumerable<DialogScriptToken>
    {
        /// <summary>
        /// List of script tokens
        /// </summary>
        public ReferenceReadOnlyList<DialogScriptToken> Tokens
        {
            get
            {
                field ??= new(_tokens);
                return field;
            }
        }

        private readonly string _source = source;
        private int _position = 0;
        private int _line = 1;
        private int _column = 1;
        private readonly List<DialogScriptToken> _tokens = [];

        #region Управление

        /// <summary>
        /// Parse script source to tokens
        /// </summary>
        public void Tokenize()
        {
            _tokens.Clear();

            while (!IsEndOfFile())
            {
                char current = Peek();

                if (char.IsWhiteSpace(current))
                {
                    SkipWhitespace();
                    continue;
                }
                if (current == '/' && PeekNext() == '/')
                {
                    ReadSingleLineComment();
                    continue;
                }
                if (current == '/' && PeekNext() == '*')
                {
                    ReadMultiLineComment();
                    continue;
                }
                if (current == '"')
                {
                    ReadString();
                    continue;
                }
                if (char.IsDigit(current) || (current == '-' && char.IsDigit(PeekNext())))
                {
                    ReadNumber();
                    continue;
                }
                if (char.IsLetter(current) || current == '_')
                {
                    ReadIdentifierOrKeyword();
                    continue;
                }

                ReadOperatorOrPunctuation();
            }

            AddToken(DialogScriptTokenType.EndOfFile, "");
        }

        #endregion

        #region Чтение кода

        private bool IsEndOfFile() => _position >= _source.Length;
        private char Peek() => IsEndOfFile() ? '\0' : _source[_position];
        private char PeekNext() => _position + 1 >= _source.Length ? '\0' : _source[_position + 1];
        private char GetNext()
        {
            char value = Peek();
            _position++;

            if (value == '\n')
            {
                _line++;
                _column = 1;
            }
            else if (value != '\r')
            {
                _column++;
            }

            return value;
        }

        private void SkipWhitespace()
        {
            while (!IsEndOfFile() && char.IsWhiteSpace(Peek()))
            {
                GetNext();
            }
        }
        private void ReadSingleLineComment()
        {
            int startLine = _line;
            int startCol = _column;
            StringBuilder builder = new();

            GetNext();
            GetNext();

            while (!IsEndOfFile() && Peek() != '\n')
            {
                builder.Append(GetNext());
            }

            AddToken(DialogScriptTokenType.Comment, builder.ToString(), startLine, startCol);
        }
        private void ReadMultiLineComment()
        {
            int startLine = _line;
            int startCol = _column;
            StringBuilder builder = new();

            GetNext();
            GetNext();

            while (!IsEndOfFile())
            {
                if (Peek() == '*' && PeekNext() == '/')
                {
                    GetNext();
                    GetNext();
                    break;
                }

                builder.Append(GetNext());
            }

            AddToken(DialogScriptTokenType.MultilineComment, builder.ToString(), startLine, startCol);
        }
        private void ReadString()
        {
            int startLine = _line;
            int startCol = _column;
            StringBuilder builder = new();

            GetNext();

            while (!IsEndOfFile() && Peek() != '"')
            {
                if (Peek() == '\\')
                {
                    GetNext();
                    char escape = Peek();

                    switch (escape)
                    {
                        case 'n': builder.Append('\n'); break;
                        case 't': builder.Append('\t'); break;
                        case 'r': builder.Append('\r'); break;
                        case '\\': builder.Append('\\'); break;
                        case '"': builder.Append('"'); break;
                        default: builder.Append(escape); break;
                    }

                    GetNext();
                }
                else
                {
                    builder.Append(GetNext());
                }
            }

            GetNext();
            AddToken(DialogScriptTokenType.StringLiteral, builder.ToString(), startLine, startCol);
        }
        private void ReadNumber()
        {
            int startLine = _line;
            int startCol = _column;
            StringBuilder builder = new();
            bool hasDot = false;

            if (Peek() == '-')
            {
                builder.Append(GetNext());
            }

            while (!IsEndOfFile() && (char.IsDigit(Peek()) || (Peek() == '.' && !hasDot)))
            {
                hasDot = Peek() == '.';
                builder.Append(GetNext());
            }

            AddToken(DialogScriptTokenType.NumberLiteral, builder.ToString(), startLine, startCol);
        }
        private void ReadIdentifierOrKeyword()
        {
            int startLine = _line;
            int startCol = _column;
            StringBuilder builder = new();

            while (!IsEndOfFile() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
            {
                builder.Append(GetNext());
            }

            string value = builder.ToString();
            DialogScriptTokenType type = Keywords.GetValueOrDefault(value, DialogScriptTokenType.Identifier);

            AddToken(type, value, startLine, startCol);
        }
        private void ReadOperatorOrPunctuation()
        {
            int startLine = _line;
            int startColumn = _column;
            char current = GetNext();

            switch (current)
            {
                case '=':
                    var next = Peek();
                    if (next == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.Equal, "==", startLine, startColumn);
                    }
                    else if (next == '>')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.Lambda, "=>", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Assign, "=", startLine, startColumn);
                    }

                    break;
                case '!':
                    if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.NotEqual, "!=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Not, "!", startLine, startColumn);
                    }

                    break;
                case '<':
                    if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.LessEqual, "<=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Less, "<", startLine, startColumn);
                    }

                    break;
                case '>':
                    if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.GreaterEqual, ">=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Greater, ">", startLine, startColumn);
                    }

                    break;
                case '&':
                    if (Peek() == '&')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.And, "&&", startLine, startColumn);
                    }
                    break;
                case '|':
                    if (Peek() == '|')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.Or, "||", startLine, startColumn);
                    }
                    break;
                case '+':
                    if (Peek() == '+')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.Increment, "++", startLine, startColumn);
                    }
                    else if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.PlusAssign, "+=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Plus, "+", startLine, startColumn);
                    }

                    break;
                case '-':
                    if (Peek() == '-')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.Decrement, "--", startLine, startColumn);
                    }
                    else if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.MinusAssign, "-=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Minus, "-", startLine, startColumn);
                    }

                    break;
                case '*':
                    if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.MultiplyAssign, "*=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Multiply, "*", startLine, startColumn);
                    }

                    break;
                case '/':
                    if (Peek() == '=')
                    {
                        GetNext();
                        AddToken(DialogScriptTokenType.MultiplyAssign, "/=", startLine, startColumn);
                    }
                    else
                    {
                        AddToken(DialogScriptTokenType.Divide, "/", startLine, startColumn);
                    }

                    break;
                case '%':
                    AddToken(DialogScriptTokenType.Mod, "%", startLine, startColumn);
                    break;
                case '(':
                    AddToken(DialogScriptTokenType.LeftParen, "(", startLine, startColumn);
                    break;
                case ')':
                    AddToken(DialogScriptTokenType.RightParen, ")", startLine, startColumn);
                    break;
                case '{':
                    AddToken(DialogScriptTokenType.LeftBrace, "{", startLine, startColumn);
                    break;
                case '}':
                    AddToken(DialogScriptTokenType.RightBrace, "}", startLine, startColumn);
                    break;
                case '[':
                    AddToken(DialogScriptTokenType.LeftBracket, "[", startLine, startColumn);
                    break;
                case ']':
                    AddToken(DialogScriptTokenType.RightBracket, "]", startLine, startColumn);
                    break;
                case ',':
                    AddToken(DialogScriptTokenType.Comma, ",", startLine, startColumn);
                    break;
                case '.':
                    AddToken(DialogScriptTokenType.Dot, ".", startLine, startColumn);
                    break;
                case ';':
                    AddToken(DialogScriptTokenType.Semicolon, ";", startLine, startColumn);
                    break;
                case ':':
                    AddToken(DialogScriptTokenType.Colon, ":", startLine, startColumn);
                    break;
                case '@':
                    AddToken(DialogScriptTokenType.At, "@", startLine, startColumn);
                    break;
                case '?':
                    AddToken(DialogScriptTokenType.Question, "?", startLine, startColumn);
                    break;
                default:
                    throw new Exception($"Unknown character '{current}' at {_line}:{_column}");
            }
        }
        private void AddToken(DialogScriptTokenType type, string value, int line = -1, int column = -1)
        {
            _tokens.Add(new(type, value,
                            line == -1 ? _line : line,
                            column == -1 ? _column : column));
        }

        #endregion

        #region Перечисление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IEnumerator<DialogScriptToken> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Статика

        private static ReadOnlyDictionary<string, DialogScriptTokenType> Keywords
        {
            get
            {
                if (field == null)
                {
                    Dictionary<string, DialogScriptTokenType> keywords = [];

                    foreach (var tokenType in Enum.GetValues(typeof(DialogScriptTokenType)))
                    {
                        var keywordAttribute = tokenType.GetEnumAttribute<KeywordAttribute>();

                        if (keywordAttribute != null)
                        {
                            keywords.Add(keywordAttribute.Name, (DialogScriptTokenType)tokenType);
                        }
                    }

                    field = new(keywords);
                }

                return field;
            }
        }

        #endregion
    }
}
