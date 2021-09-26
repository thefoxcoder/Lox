using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Scanner
    {
        public readonly string Source;
        public readonly List<Token> Tokens = new();
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private readonly Dictionary<string, TokenType> _keyWords = new Dictionary<string, TokenType>
        {
            {"and", TokenType.AND},
            {"class", TokenType.CLASS},
            {"else", TokenType.ELSE},
            {"false", TokenType.FALSE},
            {"for", TokenType.FOR},
            {"fun", TokenType.FUN},
            {"if", TokenType.IF},
            {"nil", TokenType.NIL},
            {"or", TokenType.OR},
            {"print", TokenType.PRINT},
            {"return", TokenType.RETURN},
            {"super", TokenType.SUPER},
            {"this", TokenType.THIS},
            {"true", TokenType.TRUE},
            {"var", TokenType.VAR},
            {"while", TokenType.WHILE}
        };

        public Scanner(string source)
        {
            Source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                _start = _current;
                ScanToken();
            }

            Tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return Tokens;
        }

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else if (Match('*'))
                    {
                        while (!IsAtEnd())
                        {
                            if (Peek() == '*' && PeekNext() == '/')
                            {
                                Advance();
                                Advance();
                                break;
                            }
                         
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '\n':
                    _line++;
                    break;
                case '"': GetString(); break;
                default:
                    if (IsDigit(c))
                    {
                        GetNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void GetNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER,
                double.Parse(Source.Substring(_start, _current - _start)));
        }

        private void GetString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(_line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            var value = Source.Substring(_start + 1, _current - _start - 1);
            AddToken(TokenType.STRING, value);
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            var text = Source.Substring(_start, _current - _start);

            if (!_keyWords.TryGetValue(text, out var type))
            {
                type = TokenType.IDENTIFIER;
            }

            AddToken(type);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (Source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return Source[_current];
        }

        private char PeekNext()
        {
            return _current + 1 >= Source.Length
                ? '\0'
                : Source[_current + 1];
        }

        private bool IsAlpha(char c)
        {
            return (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_');
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsDigit(char c)
        {
            return c is >= '0' and <= '9';
        }

        private bool IsAtEnd()
        {
            return _current >= Source.Length;
        }

        private char Advance()
        {
            return Source[_current++];
        }

        private void AddToken(TokenType type, object literal = null)
        {
            var text = Source.Substring(_start, _current - _start);
            Tokens.Add(new Token(type, text, literal, _line));
        }
    }
}
